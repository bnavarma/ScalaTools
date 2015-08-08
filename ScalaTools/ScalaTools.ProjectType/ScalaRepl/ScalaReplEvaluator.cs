using Microsoft.VisualStudio.Text;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Microsoft.ScalaTools.Repl
{
    [ReplRole("Reset"), ReplRole("Execution")]
    sealed class ScalaReplEvaluator : IReplEvaluator
    {
        private ListenerThread _listener;
        private IReplWindow _window;
        private readonly IScalaReplSite _site;
        internal static readonly object InputBeforeReset = new object();

        public ScalaReplEvaluator() : this(VsScalaReplSite.Site) { }

        public ScalaReplEvaluator(IScalaReplSite site) { _site = site; }

        #region IReplEvaluator Members

        public Task<ExecutionResult> Initialize(IReplWindow window)
        {
            _window = window;
            _window.SetOptionValue(ReplOptions.CommandPrefix, ".");
            _window.SetOptionValue(ReplOptions.PrimaryPrompt, "> ");
            _window.SetOptionValue(ReplOptions.SecondaryPrompt, ". ");
            _window.SetOptionValue(ReplOptions.DisplayPromptInMargin, false);
            _window.SetOptionValue(ReplOptions.SupportAnsiColors, true);
            _window.SetOptionValue(ReplOptions.UseSmartUpDown, true);

            _window.WriteLine("Avinash is King");

            return ExecutionResult.Succeeded;
        }

        //public void ActiveLanguageBufferChanged(ITextBuffer currentBuffer, ITextBuffer previousBuffer)
        //{
        //    int i = 10;
        //}

        public Task<ExecutionResult> Reset()
        {
            var bufferBeforeReset = _window.TextView.BufferGraph.GetTextBuffers(TruePredicate);
            for (int i = 0; i < bufferBeforeReset.Count - 1; i++)
            {
                var buffer = bufferBeforeReset[i];
                if (!buffer.Properties.ContainsProperty(InputBeforeReset))
                {
                    buffer.Properties.AddProperty(InputBeforeReset, InputBeforeReset);
                }
            }
            Connect();
            return ExecutionResult.Succeeded;
        }

        private static bool TruePredicate(ITextBuffer buffer) { return true; }

        public bool CanExecuteText(string text)
        {
            //var errorSink = new ReplErrorSink(text);
            //return !errorSink.Unterminated;
            return true;
        }

        public Task<ExecutionResult> ExecuteText(string text)
        {
            EnsureConnected();
            if (_listener == null)
            {
                return ExecutionResult.Failed;
            }
            return _listener.ExecuteText(text);
        }

        public void ExecuteFile(string filename)
        {
            throw new NotImplementedException();
        }

        public string FormatClipboard()
        {
            return Clipboard.GetText();
        }

        public void AbortCommand()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            if (_listener != null)
            {
                _listener.Dispose();
            }
        }

        #endregion

        private void EnsureConnected()
        {
            if (_listener == null)
            {
                Connect();
            }
        }

        private void Connect()
        {
            if (_listener != null)
            {
                _listener.Disconnect();
                _listener.Dispose();
                _listener = null;
            }

            string scalaExePath = GetScalaExePath();
            if (String.IsNullOrWhiteSpace(scalaExePath))
            {
                _window.WriteError("Fuck this shit");
                _window.WriteError(Environment.NewLine);
                return;
            }
            else if (!File.Exists(scalaExePath))
            {
                _window.WriteError("Still couldn't find this shit");
                _window.WriteError(Environment.NewLine);
                return;
            }

            Socket socket;
            int port;
            CreateConnection(out socket, out port);

            var psi = new ProcessStartInfo(scalaExePath);
            psi.CreateNoWindow = true;
            psi.UseShellExecute = false;
            psi.RedirectStandardError = true;
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardInput = true;

            string fileName, directory = null;

            if (_site.TryGetStartupFileAndDirectory(out fileName, out directory))
            {
                psi.WorkingDirectory = directory;
                //psi.EnvironmentVariables[]
            }

            var process = new Process();
            process.StartInfo = psi;
            try
            {
                process.Start();
            }
            catch (Exception e)
            {
                _window.WriteError(String.Format("Failed to start interactive process: {0}{1}{2}", Environment.NewLine, e.ToString(), Environment.NewLine));
                return;
            }

            _listener = new ListenerThread(this, process, socket);
            //CreateCommandProcessor(socket, psi.RedirectStandardOutput, process);
        }

        private string GetScalaExePath()
        {
            var startupProject = _site.GetStartupProject();
            string scalaExePath;
            if (startupProject != null)
            {
                //scalaExePath = startupProject.GetProjectProperty(ScalaConstants)
            }
            scalaExePath = @"C:\Program Files (x86)\scala\bin\scala.bat";
            return scalaExePath;
        }

        private static void CreateConnection(out Socket conn, out int portNum)
        {
            conn = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
            conn.Bind(new IPEndPoint(IPAddress.Loopback, 0));
            conn.Listen(0);
            portNum = ((IPEndPoint)conn.LocalEndPoint).Port;
        }

        protected void CreateCommandProcessor(Stream stream, bool redirectStdOutput, Process process)
        {
            //_listener = new ListenerThread(this, stream, redirectStdOutput, process);
        }

        //public void ActiveLanguageBufferChanged(ITextBuffer currentBuffer, ITextBuffer previousBuffer)
        //{
        //    throw new NotImplementedException();
        //}

        //class ListenerThread : IDisposable
        //{
        //    private readonly ScalaReplEvaluator _eval;

        //    private Socket _listenerSocket;
        //    private Stream _stream;

        //    private readonly object _streamLock = new object();

        //    private TaskCompletionSource<ExecutionResult> _completion;
        //    private readonly object _completionLock = new object();
        //    private Action _deferredExecute;

        //    private readonly Process _process;
        //    //private AutoResetEvent _completionResultEvent = 

        //    public ListenerThread
        //}

        class ListenerThread : JsonListener, IDisposable
        {
            private readonly ScalaReplEvaluator _eval;
            private readonly Process _process;
            private readonly object _socketLock = new object();
            private Socket _acceptSocket;
            private Stream _stream;
            internal bool _connected;
            private TaskCompletionSource<ExecutionResult> _completion;
            private string _executionText;
            //private readonly ScalaSerializer
            private bool _disposed;

            static string _noReplProcess = "Current interactive window in disconnected - please reset the process." + Environment.NewLine;

            public ListenerThread(ScalaReplEvaluator eval, Process process, Socket socket)
            {
                _eval = eval;
                _process = process;
                _acceptSocket = Socket;

                _acceptSocket.BeginAccept(SocketConnectionAccepted, null);

                _process.OutputDataReceived += new DataReceivedEventHandler(StdOutReceived);
                _process.ErrorDataReceived += new DataReceivedEventHandler(StdErrReceived);
                _process.EnableRaisingEvents = true;
                _process.Exited += ProcessExited;

                _process.BeginOutputReadLine();
                _process.BeginErrorReadLine();
            }

            private void StdOutReceived(object sender, DataReceivedEventArgs args)
            {
                if (args.Data != null)
                {
                    _eval._window.WriteOutput(args.Data + Environment.NewLine);
                }
            }

            private void StdErrReceived(object sender, DataReceivedEventArgs args)
            {
                if (args.Data != null)
                {
                    _eval._window.WriteError(args.Data + Environment.NewLine);
                }
            }

            private void ProcessExited(object sender, EventArgs args)
            {
                ProcessExitedWorker();
            }

            private void ProcessExitedWorker()
            {
                _eval._window.WriteError("The process has exited" + Environment.NewLine);
                using (new SocketLock(this))
                {
                    if (_completion != null)
                    {
                        _completion.SetResult(ExecutionResult.Failure);
                    }
                    _completion = null;
                }
            }

            private void SocketConnectionAccepted(IAsyncResult result)
            {
                Socket = _acceptSocket.EndAccept(result);
                _acceptSocket.Close();

                using (new SocketLock(this))
                {
                    _connected = true;
                }

                using (new SocketLock(this))
                {
                    if (_executionText != null)
                    {
                        Debug.WriteLine("Executing delayed text: " + _executionText);
                        SendExecuteText(_executionText);
                        _executionText = null;
                        _stream = new NetworkStream(Socket, ownsSocket: true);
                    }
                }

                //StartListenerThread();
            }

            public Task<ExecutionResult> ExecuteText(string text)
            {
                TaskCompletionSource<ExecutionResult> completion;
                Debug.WriteLine("Executing text: " + text);
                using (new SocketLock(this))
                {
                    if (!_connected)
                    {
                        Debug.WriteLine("Delayed executing text");
                        _completion = completion = new TaskCompletionSource<ExecutionResult>();
                        _executionText = text;
                        return completion.Task;
                    }

                    try
                    {
                        if (!Socket.Connected)
                        {
                            _eval._window.WriteError(_noReplProcess);
                            return ExecutionResult.Failed;
                        }

                        _completion = completion = new TaskCompletionSource<ExecutionResult>();

                        SendExecuteText(text);
                    }
                    catch (SocketException)
                    {
                        _eval._window.WriteError(_noReplProcess);
                        return ExecutionResult.Failed;
                    }

                    return completion.Task;
                }
            }

            [DllImport("user32", CallingConvention = CallingConvention.Winapi)]
            static extern bool AllowSetForegroundWindow(int dwProcessId);

            private void SendExecuteText(string text)
            {
                AllowSetForegroundWindow(_process.Id);
                var request = new Dictionary<string, object>()
                {
                    {"type","execute" },
                    {"code",text },
                };
                SendRequest();
            }

            internal void SendRequest()
            {
                string input = _eval._window.ReadStandardInput();
                //input = input != null ? UnfixNewLines(input) : "\n";
                //try
                //{
                //    using (new SocketLock(this))
                //    {
                //        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(input);
                //        _stream.WriteInt32(bytes.Length);
                //        _stream.Write(bytes);
                //    }
                //}
                //catch (IOException) { }
                _process.StandardInput.WriteAsync(input);

            }

            private string UnfixNewLines(string input)
            {
                StringBuilder res = new StringBuilder();
                for (int i = 0; i < input.Length; i++)
                {
                    if (input[i] == '\r' && i != input.Length - 1 && input[i + 1] == '\n')
                    {
                        res.Append('\n');
                        i++;
                    }
                    else
                    {
                        res.Append(input[i]);
                    }
                }
                return res.ToString();
            }

            private static string FixOutput(object result)
            {
                var res = result.ToString();
                if (res.IndexOf('\n') != -1)
                {
                    StringBuilder fixedStr = new StringBuilder();
                    for (int i = 0; i < res.Length; i++)
                    {
                        if (res[i] == '\r')
                        {
                            if (i + 1 < res.Length && res[i + 1] == '\n')
                            {
                                i++;
                                fixedStr.Append("\r\n");
                            }
                            else
                            {
                                fixedStr.Append("\r\n");
                            }
                        }
                        else if (res[i] == '\n')
                        {
                            fixedStr.Append("\r\n");
                        }
                        else
                        {
                            fixedStr.Append(res[i]);
                        }
                    }
                    res = fixedStr.ToString();
                }
                return res;
            }

            internal void Disconnect()
            {
                if (_completion != null)
                {
                    _completion.SetResult(ExecutionResult.Failure);
                    _completion = null;
                }
            }

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            protected virtual void Dispose(bool disposing)
            {
                if (!_disposed)
                {
                    if (_process != null && !_process.HasExited)
                    {
                        try
                        {
                            _process.Exited -= ProcessExited;
                            _process.Kill();
                        }
                        catch (InvalidOperationException) { }
                        catch (NotSupportedException) { }
                        catch (Win32Exception) { }
                        ProcessExitedWorker();
                    }

                    if (_process != null)
                    {
                        _process.Dispose();
                    }
                    _disposed = true;
                }
            }

            protected override void OnSocketDisconnected()
            {
                throw new NotImplementedException();
            }

            protected override void ProcessPacket(JsonResponse response)
            {
                throw new NotImplementedException();
            }

            struct SocketLock : IDisposable
            {
                private readonly ListenerThread _evaluator;
                public SocketLock(ListenerThread evaluator)
                {
                    Monitor.Enter(evaluator._socketLock);
                    _evaluator = evaluator;
                }

                public void Dispose()
                {
                    Monitor.Exit(_evaluator._socketLock);
                }
            }
        }
    }
}
