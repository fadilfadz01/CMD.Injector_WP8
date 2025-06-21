using SocketEx;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WUT_WP8.UWP;

namespace WUT_WP8.UWP
{
    enum Verbs
    {
        WILL = 251,
        WONT = 252,
        DO = 253,
        DONT = 254,
        IAC = 255
    }

    enum Options
    {
        SGA = 3
    }
    public class TelnetClient : IDisposable
    {
        private int _port;
        private readonly TimeSpan _sendRate;
        private readonly SemaphoreSlim _sendRateLimit;
        private readonly CancellationTokenSource _internalCancellation;

        private TcpClient _tcpClient;
        private StreamReader _tcpReader;
        private StreamWriter _tcpWriter;

        public EventHandler<string> ErrorReceived;
        public EventHandler<string> MessageReceived;
        public EventHandler ConnectionClosed;

        string[] securityCheck = new string[]
        {
            "\\.\\globalroot\\device\\condrv\\kernelconnect",
            "\\\\.\\\\globalroot\\\\device\\\\condrv\\\\kernelconnect",
            "\"%0|%0\""
        };

        string TelnetPort = "9999";
        string TelnetIP = "127.0.0.1";

        /// <summary>
        /// Simple telnet client
        /// </summary>
        /// <param name="host">Destination Hostname or IP</param>
        /// <param name="port">Destination TCP port number</param>
        /// <param name="sendRate">Minimum time span between sends. This is a throttle to prevent flooding the server.</param>
        /// <param name="token"></param>
        public TelnetClient(TimeSpan sendRate, CancellationToken token)
        {
            _port = 9999;
            _sendRate = sendRate;
            _sendRateLimit = new SemaphoreSlim(1);
            _internalCancellation = new CancellationTokenSource();

            token.Register(() => _internalCancellation.Cancel());
        }

        /// <summary>
        /// Connect and wait for incoming messages. 
        /// When this task completes you are connected. 
        /// You cannot call this method twice; if you need to reconnect, dispose of this instance and create a new one.
        /// </summary>
        /// <returns></returns>
        public void Connect()
        {
            if (_tcpClient != null)
            {
                OnErrorReceived("Connect aborted: Reconnecting is not supported. You must dispose of this instance and instantiate a new TelnetClient");
                return;
            }
            try
            {
                _port = Int16.Parse(TelnetPort);
            }
            catch (Exception e)
            {

            }
            _tcpClient = new TcpClient();
            _tcpClient.Connect(TelnetIP, _port);

            _tcpReader = new StreamReader(_tcpClient.GetStream());
            _tcpWriter = new StreamWriter(_tcpClient.GetStream()) { AutoFlush = true };

            // Fire-and-forget looping task that waits for messages to arrive
            Task.Run(() =>
            {
                WaitForMessage();
            });
        }
        public void Connect(string TelnetIP, int _port)
        {
            if (_tcpClient != null)
            {
                OnErrorReceived("Connect aborted: Reconnecting is not supported. You must dispose of this instance and instantiate a new TelnetClient");
                return;
            }

            _tcpClient = new TcpClient();
            _tcpClient.Connect(TelnetIP, _port);

            _tcpReader = new StreamReader(_tcpClient.GetStream());
            _tcpWriter = new StreamWriter(_tcpClient.GetStream()) { AutoFlush = true };

            // Fire-and-forget looping task that waits for messages to arrive
            Task.Run(() =>
            {
                WaitForMessage();
            });
        }


        public void Send(string message)
        {
            try
            {
                foreach (var sItem in securityCheck)
                {
                    if (message.Trim().ToLower().Contains(sItem.ToLower()))
                    {
                        OnErrorReceived("Command blocked due security concerns");
                        return;
                    }
                }
                if (message.Trim().StartsWith("[") || message.Trim().StartsWith("]") || message.Trim().Equals("}") || message.Trim().Equals("//") || message.Trim().Equals("/*") || message.Trim().Equals("#") || message.Trim().Equals("{") || message.Trim().ToLower().StartsWith("rem ") || message.Trim().ToLower().StartsWith("::") || message.Trim().EndsWith("++") || message.Trim().EndsWith("--"))
                {
                    return;
                }
                if (message.StartsWith("cmd:"))
                {
                    message = message.Replace("cmd:", "");
                }
                if (!message.EndsWith("\r\n"))
                {
                    message = message + "\r\n";
                }
                // Wait for any previous send commands to finish and release the semaphore
                // This throttles our commands
                _sendRateLimit.Wait(_internalCancellation.Token);

                // Send command + params
                _tcpWriter.WriteLine(message);

                // Block other commands until our timeout to prevent flooding
                Task.Delay(_sendRate, _internalCancellation.Token);
                try
                {
                    OnMessageReceived(message.Replace("\r\n", ""));
                }
                catch (Exception e)
                {

                }
            }
            catch (OperationCanceledException)
            {
                // We're waiting to release our semaphore, and someone cancelled the task on us (I'm looking at you, WaitForMessages...)
                // This happens if we've just sent something and then disconnect immediately
                OnErrorReceived("Send aborted: IsCancellationRequested == true");
            }
            catch (ObjectDisposedException)
            {
                // This happens during ReadLineAsync() when we call Disconnect() and close the underlying stream
                // This is an expected exception during disconnection if we're in the middle of a send
                OnErrorReceived("Send failed: _tcpWriter or _tcpWriter.BaseStream disposed");
            }
            catch (IOException)
            {
                // This happens when we start WriteLineAsync() if the socket is disconnected unexpectedly
                OnErrorReceived("Send failed: Socket disconnected unexpectedly");
            }
            catch (Exception error)
            {
                OnErrorReceived("Send failed: " + error);
            }
            finally
            {
                // Exit our semaphore
                _sendRateLimit.Release();
            }
        }

        private void WaitForMessage()
        {
            try
            {
                while (true)
                {
                    if (_internalCancellation.IsCancellationRequested)
                    {
                        OnErrorReceived("WaitForMessage aborted: IsCancellationRequested == true");
                        break;
                    }

                    string message;

                    try
                    {
                        if (!_tcpClient.Connected)
                        {
                            OnErrorReceived("WaitForMessage aborted: _tcpClient is not connected");
                            break;
                        }

                        // Due to CR/LF platform differences, we sometimes get empty messages if the server sends us over-eager EOL markers
                        // Because ReadLine*() strips out the EOL characters, the message can end up empty (but not null!)
                        // I *think* this is a server implementation problem rather than our problem to solve
                        // So just handle empty messages in your consumer library
                        message = _tcpReader.ReadLine();

                        if (message == null)
                        {
                            OnErrorReceived("WaitForMessage aborted: _tcpReader reached end of stream");
                            break;
                        }
                        else
                        {

                        }
                    }
                    catch (ObjectDisposedException)
                    {
                        // This happens during ReadLineAsync() when we call Disconnect() and close the underlying stream
                        // This is an expected exception during disconnection
                        OnErrorReceived("WaitForMessage aborted: _tcpReader or _tcpReader.BaseStream disposed. This is expected after calling Disconnect()");
                        break;
                    }
                    catch (IOException)
                    {
                        // This happens when we start ReadLineAsync() if the socket is disconnected unexpectedly
                        OnErrorReceived("WaitForMessage aborted: Socket disconnected unexpectedly");
                        break;
                    }
                    catch (Exception error)
                    {
                        OnErrorReceived("WaitForMessage aborted: " + error);
                        break;
                    }

                    OnErrorReceived("WaitForMessage received: message [" + message.Length + "]");

                    try
                    {
                        OnMessageReceived(message);
                    }
                    catch (Exception ee)
                    {

                    }
                    Task.Delay(1);
                }
            }
            finally
            {
                OnErrorReceived("WaitForMessage completed: Calling Disconnect");
                Disconnect();
            }
        }

        void ParseTelnet(StringBuilder sb)
        {
            while (_tcpClient.Available > 0)
            {
                int input = _tcpClient.GetStream().ReadByte();
                switch (input)
                {
                    case -1:
                        break;
                    case (int)Verbs.IAC:
                        // interpret as command
                        int inputverb = _tcpClient.GetStream().ReadByte();
                        if (inputverb == -1) break;
                        switch (inputverb)
                        {
                            case (int)Verbs.IAC:
                                //literal IAC = 255 escaped, so append char 255 to string
                                sb.Append(inputverb);
                                break;

                            case (int)Verbs.DO:
                            case (int)Verbs.DONT:
                            case (int)Verbs.WILL:
                            case (int)Verbs.WONT:
                                // reply to all commands with "WONT", unless it is SGA (suppres go ahead)
                                int inputoption = _tcpClient.GetStream().ReadByte();

                                if (inputoption == -1) break;

                                _tcpClient.GetStream().WriteByte((byte)Verbs.IAC);

                                if (inputoption == (int)Options.SGA)
                                {
                                    _tcpClient.GetStream().WriteByte
                                    (
                                        inputverb == (int)Verbs.DO ? (byte)Verbs.WILL : (byte)Verbs.DO
                                    );
                                }
                                else
                                {
                                    _tcpClient.GetStream().WriteByte
                                    (
                                        inputverb == (int)Verbs.DO ? (byte)Verbs.WONT : (byte)Verbs.DONT
                                    );
                                }

                                _tcpClient.GetStream().WriteByte((byte)inputoption);
                                break;

                            default:
                                break;
                        }
                        break;

                    default:
                        sb.Append((char)input);
                        break;
                }
            }
        }
        /// <summary>
        /// Disconnecting will leave TelnetClient in an unusable state.
        /// </summary>
        public void Disconnect()
        {
            try
            {
                // Blow up any outstanding tasks
                _internalCancellation.Cancel();

                // Both reader and writer use the TcpClient.GetStream(), and closing them will close the underlying stream
                // So closing the stream for TcpClient is redundant
                // But it means we're triple sure!
                _tcpReader.Dispose();
                _tcpWriter.Dispose();
                _tcpClient.Dispose();
            }
            catch (Exception error)
            {
                OnErrorReceived("Disconnect error: " + error);
            }
            finally
            {
                OnConnectionClosed();
            }
        }

        public bool IsConnected
        {
            get { return _tcpClient.Connected; }
        }


        private void OnMessageReceived(string message)
        {
            EventHandler<string> messageReceived = MessageReceived;

            if (messageReceived != null)
            {
                messageReceived(this, message);
            }
        }

        private void OnErrorReceived(string message)
        {
            EventHandler<string> messageReceived = ErrorReceived;

            if (messageReceived != null)
            {
                messageReceived(this, message);
            }
        }

        private void OnConnectionClosed()
        {
            EventHandler connectionClosed = ConnectionClosed;

            if (connectionClosed != null)
            {
                connectionClosed(this, new EventArgs());
            }
        }

        private bool _disposed = false;


        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                Disconnect();
            }

            _disposed = true;
        }
    }
}
