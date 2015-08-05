using Microsoft.VisualStudio.Text;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Microsoft.ScalaTools.Repl
{
    [ReplRole("Reset"),ReplRole("Execution")]
    sealed class ScalaReplEvaluator:IReplEvaluator
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

        public void ActiveLanguageBufferChanged(ITextBuffer currentBuffer, ITextBuffer previousBuffer) { }

        public Task<ExecutionResult> Reset()
        {
            var bufferBeforeReset = _window.TextView.BufferGraph.GetTextBuffers(TruePredicate);
            for(int i =0;i<bufferBeforeReset.Count - 1; i++)
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

        public bool CanExecuteText(string text) {
            //var errorSink = new ReplErrorSink(text);
            //return !errorSink.Unterminated;
            return true;
        }

        public Task<ExecutionResult> ExecuteText(string text)
        {
            EnsureConnected();
            if(_listener == null)
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
            if(_listener != null)
            {
                _listener.Dispose();
            }
        }

        #endregion

        private void EnsureConnected()
        {
            if(_listener == null)
            {
                Connect();
            }
        }

        private void Connect()
        {
            if(_listener != null)
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

            var psi = new ProcessStartInfo(scalaExePath, port);
            psi.CreateNoWindow = true;
            psi.UseShellExecute = false;
            psi.RedirectStandardError = true;
            psi.RedirectStandardOutput = true;

            string fileName, directory = null;

            if(_site.TryGetStartupFileAndDirectory(out fileName, out directory))
            {
                psi.WorkingDirectory = directory;
                //psi.EnvironmentVariables[]
            }

            var process = new Process();
            process.StartInfo = psi;
            try
            {
                process.Start();
            }catch(Exception e)
            {
                _window.WriteError(String.Format("Failed to start interactive process: {0}{1}{2}", Environment.NewLine, e.ToString(), Environment.NewLine));
                return;
            }

            _listener = new ListenerThread(this, process, socket);
        }

        private string GetScalaExePath()
        {
            var startupProject = _site.GetStartupProject();
            string scalaExePath;
            if(startupProject != null)
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

        class ListenerThread : JsonListener, IDisposable
        {
            private readonly ScalaReplEvaluator _eval;
            private readonly Process _process;
            private readonly object _socketLock = new object();
            private Socket _acceptSocket;
            internal bool _connected;
            private TaskCompletionSource<ExecutionResult> _completion;
            private string _executionText;
            //private readonly ScalaSerializer
            private bool _disposed;
        }
    }
}
