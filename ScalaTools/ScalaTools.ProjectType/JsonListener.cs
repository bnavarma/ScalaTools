﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Microsoft.ScalaTools
{
    abstract class JsonListener
    {
        private readonly byte[] _socketBuffer = new byte[4096];
        private Socket _socket;

        protected void StartListenerThread()
        {
            var debuggerThread = new Thread(ListenerThread);
            debuggerThread.Name = GetType().Name + " Thread";
            debuggerThread.Start();
        }

        private void ListenerThread()
        {
            int pos = 0;
            byte[] text = new byte[0];

            var socket = _socket;

            try
            {
                if (socket != null && socket.Connected)
                {
                    while (_socket != null && socket.Connected)
                    {
                        if (pos >= text.Length)
                        {
                            ReadMoreData(socket.Receive(_socketBuffer), ref text, ref pos);
                        }

                        Dictionary<string, string> headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                        while (_socket != null && socket.Connected)
                        {
                            int newPos = text.FirstNewLine(pos);
                            if (newPos == pos)
                            {
                                pos += 2;
                                break;
                            }
                            else if (newPos == -1)
                            {
                                ReadMoreData(socket.Receive(_socketBuffer), ref text, ref pos);
                            }
                            else
                            {
                                int nameEnd = text.IndexOf((byte)':', pos, newPos - pos);
                                if(nameEnd != -1)
                                {
                                    var headerName = text.Substring(pos, nameEnd - pos);
                                    string headerNameStr = Encoding.UTF8.GetString(headerName).Trim();

                                    var headerValue = text.Substring(nameEnd + 1, newPos - nameEnd - 1);
                                    string headerValueStr = Encoding.UTF8.GetString(headerValue).Trim();
                                    headers[headerNameStr] = headerValueStr;
                                }
                                pos = newPos + 2;
                            }
                        }

                        string body = String.Empty;
                        string contentLen;
                        if(headers.TryGetValue("Content-Length",out contentLen))
                        {
                            int lengthRemaining = Int32.Parse(contentLen);
                            if (lengthRemaining != 0)
                            {
                                StringBuilder bodyBuilder = new StringBuilder();
                                while(_socket != null && socket.Connected)
                                {
                                    int len = Math.Min(text.Length - pos, lengthRemaining);
                                    bodyBuilder.Append(Encoding.UTF8.GetString(text.Substring(pos, len)));
                                    pos += len;

                                    lengthRemaining -= len;
                                    if(lengthRemaining == 0)
                                    {
                                        break;
                                    }
                                    ReadMoreData(socket.Receive(_socketBuffer), ref text, ref pos);
                                }
                                body = bodyBuilder.ToString();
                            }
                        }

                        if(_socket != null && socket.Connected)
                        {
                            try
                            {
                                ProcessPacket(new JsonResponse(headers, body));
                            }
                            catch(Exception e)
                            {
                                Console.WriteLine("Error: {0}", e);
                            }
                        }
                    }
                }
            }
            catch (SocketException) { }
            finally
            {
                Debug.Assert(_socket == null || !_socket.Connected);
                if(socket != null && socket.Connected)
                {
                    socket.Disconnect(false);
                }
                OnSocketDisconnected();
            }
        }

        protected abstract void OnSocketDisconnected();
        protected abstract void ProcessPacket(JsonResponse response);

        protected void ReadMoreData(int bytesRead,ref byte[] text, ref int pos)
        {
            byte[] combinedText = new byte[bytesRead + text.Length - pos];
            Buffer.BlockCopy(text, pos, combinedText, 0, text.Length - pos);
            Buffer.BlockCopy(_socketBuffer, 0, combinedText, text.Length - pos, bytesRead);
            text = combinedText;
            pos = 0;
        }

        protected Socket Socket
        {
            get { return _socket; }
            set { _socket = value; }
        }
    }

    static class ByteExtensions
    {
        public static int IndexOf(this byte[] bytes, byte ch, int start, int count)
        {
            for(int i = start; i<start+count && i < bytes.Length; i++)
            {
                if(bytes[i] == ch) { return i; }
            }
            return -1;
        }

        public static byte[] Substring(this byte[] bytes, int start, int length)
        {
            byte[] res = new byte[length];
            for(int i = 0; i < length; i++)
            {
                res[i] = bytes[i + start];
            }
            return res;
        }

        public static int FirstNewLine(this byte[] bytes,int start)
        {
            for(int i = 0; i < bytes.Length - 1; i++)
            {
                if(bytes[i] == '\r' && bytes[i + 1] == '\n')
                {
                    return i;
                }
            }
            return -1;
        }
    }

    class JsonResponse
    {
        public readonly Dictionary<string, string> Headers;
        public readonly string Body;

        public JsonResponse(Dictionary<string, string>headers, string body)
        {
            Headers = headers;
            Body = body;
        }
    }
}
