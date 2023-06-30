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

        public int ReadBufferSize { get; private set; } = 96;
        public bool IsRunning { get; private set; } = false;

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
            OnConnected();
            Client.Client.BeginReceive(_readBuffer, 0, _readBuffer.Length, SocketFlags.None, new AsyncCallback(DoReceive), null);
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
            CleanUp();
        }

        private void CleanUp()
        {
            if (!IsRunning)
                return;

            IsRunning = false;

            if (Client.Connected)
                Client.Close();

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
                        CleanUp();
                        break;

                    default:
                        if (length <= 0)
                        {
                            CleanUp();
                            break;
                        }

                        OnSocketError(code);
                        CleanUp();
                        break;

                    case SocketError.Success:
                        if (length <= 0)
                        {
                            CleanUp();
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
                    CleanUp();
                    return;
                }

                if (!IsRunning)
                    return;

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
                        CleanUp();
                        return;

                    default:
                        if (length <= 0)
                        {
                            CleanUp();
                            break;
                        }

                        OnSocketError(code);
                        CleanUp();
                        return;

                    case SocketError.Success:
                        if (length <= 0)
                        {
                            CleanUp();
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
