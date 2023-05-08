using System.Net;
using System.Net.Sockets;
using System.Runtime;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ExeNet
{
    /// <summary>
    /// Wrapper around TCPClient with unique ID for recieving/sending data
    /// </summary>
    public class TcpSession : IDisposable
    {
        public ushort ID;

        public bool IsRunning { get; private set; }
        public int ReadBufferSize { get; private set; } = 96;
        public EndPoint? RemoteEndPoint
        {
            get
            {
                try
                {
                    if (Client == null)
                        return null;

                    if (Client.Client == null)
                        return null;

                    if (!Client.Connected)
                        return null;

                    return Client.Client.RemoteEndPoint;
                }
                catch
                {
                    return null;
                }
            }
        }

        protected TcpClient Client { get; private set; }
        protected TcpServer Server { get; private set; }

        private byte[] _readBuffer = Array.Empty<byte>();
        private Thread? _readThread;

        public TcpSession(TcpServer server, TcpClient client)
        {
            Server = server;
            Client = client;
            ID = server.RequestID();

            // Client setup
            Client.SendTimeout = 3000;
            Client.ReceiveTimeout = 2000;
            Client.NoDelay = true;
        }

        public void Start()
        {
            _readBuffer = new byte[ReadBufferSize];

            IsRunning = true;
            try
            {
                _readThread = new(Run);
                _readThread.Name = "TcpSession Worker";
                _readThread.Start();
            }
            catch (Exception ex)
            {
                OnError(ex.Message);
            }
        }

        public void Send(byte[] data) => Send(data, data.Length);
        public void Send(byte[] data, int length)
        {
            if (!IsRunning || !Client.Connected)
                return;

            Client.Client.BeginSend(data, 0, length, SocketFlags.None, new AsyncCallback(DoSend), null);
        }

        public void Disconnect()
        {
            Stop();
        }

        public void Dispose()
        {
            Stop();
            GC.SuppressFinalize(this);
        }

        private void Stop()
        {
            IsRunning = false;

            if (_readThread == null)
                return;

            if (Environment.CurrentManagedThreadId == _readThread.ManagedThreadId)
            {
                ThreadPool.QueueUserWorkItem((ass) => Stop());
                return;
            }

            try
            {
                _readThread.Join(3000);
            }
            catch (Exception ex)
            {
                OnError(ex.Message);
            }

            if (Client.Connected)
                Client.Close();
        }

        private void Run()
        {
            OnConnected();

            Client.Client.BeginReceive(_readBuffer, 0, _readBuffer.Length, SocketFlags.None, new AsyncCallback(DoReceive), null);
            
            while (IsRunning)
                Thread.Sleep(100);

            OnDisconnected();

            lock (Server.Sessions)
            {
                if (Server.Sessions.Contains(this))
                    Server.Sessions.Remove(this);
            }
        }


        private void DoSend(IAsyncResult result)
        {
            try
            {
                int length = Client.Client.EndSend(result, out SocketError code);

                switch (code)
                {
                    // Ignore
                    case SocketError.TimedOut:
                        break;

                    case SocketError.Shutdown:
                    case SocketError.NetworkReset:
                    case SocketError.ConnectionRefused:
                    case SocketError.ConnectionReset:
                    case SocketError.ConnectionAborted:
                        IsRunning = false;
                        break;

                    default:
                        if (length <= 0)
                        {
                            IsRunning = false;
                            break;
                        }

                        OnSocketError(code);
                        IsRunning = false;
                        break;

                    case SocketError.Success:
                        if (length <= 0)
                        {
                            IsRunning = false;
                            break;
                        }
                        break;
                }
            }
            catch(Exception e)
            {
                OnError(e.Message);
            }
        }

        private void DoReceive(IAsyncResult result)
        {
            try
            {
                if (!Client.Connected)
                {
                    IsRunning = false;
                    return;
                }

                int length = Client.Client.EndReceive(result, out SocketError code);

                switch (code)
                {
                    // Ignore
                    case SocketError.TimedOut:
                        break;

                    case SocketError.Shutdown:
                    case SocketError.NetworkReset:
                    case SocketError.ConnectionRefused:
                    case SocketError.ConnectionReset:
                    case SocketError.ConnectionAborted:
                        IsRunning = false;
                        return;

                    default:
                        if (length <= 0)
                        {
                            IsRunning = false;
                            break;
                        }

                        OnSocketError(code);
                        IsRunning = false;
                        return;

                    case SocketError.Success:
                        if (length <= 0)
                        {
                            IsRunning = false;
                            return;
                        }
                        break;
                }

                OnData(_readBuffer, length);
            }
            catch(Exception e)
            {
                OnError(e.Message);
            }

            Client.Client.BeginReceive(_readBuffer, 0, _readBuffer.Length, SocketFlags.None, new AsyncCallback(DoReceive), null);
        }


        protected virtual void OnConnected() { }
        protected virtual void OnData(byte[] data, int length) { }
        protected virtual void OnSocketError(SocketError error) { }
        protected virtual void OnError(string message) { }
        protected virtual void Timeouted() { }
        protected virtual void OnDisconnected() { }
    }
}
