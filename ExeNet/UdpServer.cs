using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ExeNet
{
    public class UdpServer : IDisposable
    {
        public bool IsRunning { get; private set; }
        public int Port { get; private set; }

        private UdpClient _client;
        private Thread? _readThread;

        public UdpServer(int port)
        {
            Port = port;
            _client = new(port);
            _client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            
            // I fucking hate microsoft please kill yourelf
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                const int SIO_UDP_CONNRESET = -1744830452;
                byte[] inValue = new byte[] { 0 };
                byte[] outValue = new byte[] { 0 };
                _client.Client.IOControl(SIO_UDP_CONNRESET, inValue, outValue);
            }
        }

        public void Dispose()
        {
            Stop();
            GC.SuppressFinalize(this);
        }

        public void Start()
        {
            IsRunning = true;

            _readThread = new(Run);
            _readThread.Name = "UdpServer Worker";
            _readThread.Start();

            OnReady();
        }

        public int Send(IPEndPoint endpoint, byte[] data) => Send(endpoint, data, data.Length);
        public int Send(IPEndPoint endpoint, byte[] data, int length)
        {
            try
            {
                return _client.Send(data, length, endpoint);
            }
            catch(SocketException ex)
            {
                OnSocketError(endpoint, ex.SocketErrorCode);
            }
            catch(Exception ex)
            {
                OnError(endpoint, ex.Message);
            }

            return -1;
        }

        public void Stop()
        {
            IsRunning = false;
            _readThread!.Join();
        }

        private void Run()
        {
            while (IsRunning)
            {
                IPEndPoint endpoint = new(IPAddress.Any, Port);
                try
                {
                    byte[] bytes = _client.Receive(ref endpoint);
                    OnData(endpoint, bytes);
                }
                catch (SocketException ex)
                {
                    OnSocketError(endpoint, ex.SocketErrorCode);
                }
                catch(Exception ex)
                {
                    OnError(endpoint, ex.Message);
                }
            }

            _client.Close();
        }

        protected virtual void OnReady() { }
        protected virtual void OnSocketError(IPEndPoint endpoint, SocketError error) { }
        protected virtual void OnError(IPEndPoint endpoint, string message) { }
        protected virtual void OnData(IPEndPoint sender, byte[] data) { }
    }
}
