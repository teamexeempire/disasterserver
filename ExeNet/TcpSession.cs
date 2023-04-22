using System.Net;
using System.Net.Sockets;
using System.Runtime;

namespace ExeNet
{
    /// <summary>
    /// Wrapper around TCPClient with unique ID for recieving/sending data
    /// </summary>
    public class TcpSession : IDisposable
    {
        public ushort ID;

        public bool IsReading { get; private set; }
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

            IsReading = true;
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

        public bool Send(byte[] data) => Send(data, data.Length);
        public bool Send(byte[] data, int length)
        {
            if (!IsReading || !Client.Connected)
                return false;

            try
            {
                Client.Client.Send(data, 0, length, SocketFlags.None, out SocketError error);

                switch(error)
                {
                    //Ignore
                    case SocketError.Success:
                        break;

                    case SocketError.Shutdown:
                    case SocketError.NetworkReset:
                    case SocketError.ConnectionRefused:
                    case SocketError.ConnectionReset:
                    case SocketError.ConnectionAborted:
                        IsReading = false;
                        break;

                    case SocketError.TimedOut:
                        if (IsReading && Client.Connected)
                            Timeouted();
                        break;

                    default:
                        if (IsReading && Client.Connected)
                            OnSocketError(error);
                        break;
                }

                return (error == SocketError.Success);
            }
            catch (SocketException ex)
            {
                if (IsReading && Client.Connected)
                    OnSocketError(ex.SocketErrorCode);

                IsReading = false;
                return false;
            }
            catch (Exception ex)
            {
                if (IsReading && Client.Connected)
                    OnError(ex.Message);
                return false;
            }
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
            IsReading = false;

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
            try
            {
                OnConnected();

                while (IsReading)
                {
                    int length;

                    try
                    {
                        if (!Client.Connected)
                        {
                            IsReading = false;
                            break;
                        }

                        length = Client.Client.Receive(_readBuffer, 0, _readBuffer.Length, SocketFlags.None, out SocketError error);

                        switch (error)
                        {
                            // Ignore
                            case SocketError.TimedOut:
                                break;

                            case SocketError.Shutdown:
                            case SocketError.NetworkReset:
                            case SocketError.ConnectionRefused:
                            case SocketError.ConnectionReset:
                            case SocketError.ConnectionAborted:
                                IsReading = false;
                                break;

                            default:
                                if (length <= 0)
                                {
                                    IsReading = false;
                                    break;
                                }

                                OnSocketError(error);
                                IsReading = false;
                                break;

                            case SocketError.Success:
                                if (length <= 0)
                                {
                                    IsReading = false;
                                    break;
                                }
                                break;
                        }
                    }
                    catch (SocketException ex)
                    {
                        if (IsReading && Client.Connected)
                            OnSocketError(ex.SocketErrorCode);

                        IsReading = false;
                        break;
                    }
                    catch (Exception ex)
                    {
                        if (IsReading && Client.Connected)
                            OnError(ex.Message);

                        IsReading = false;
                        break;
                    }

                    OnData(_readBuffer, length);
                }
            }
            catch (ObjectDisposedException) 
            {

            }

            OnDisconnected();

            lock (Server.Sessions)
            {
                if (Server.Sessions.Contains(this))
                    Server.Sessions.Remove(this);
            }
        }


        protected virtual void OnConnected() { }
        protected virtual void OnData(byte[] data, int length) { }
        protected virtual void OnSocketError(SocketError error) { }
        protected virtual void OnError(string message) { }
        protected virtual void Timeouted() { }
        protected virtual void OnDisconnected() { }
    }
}
