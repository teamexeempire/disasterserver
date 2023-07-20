using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
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
        }

        public void Dispose()
        {
            Stop();
            GC.SuppressFinalize(this);
        }

        public bool Start()
        {
            try
            {
                _client = new(Port);
                _client.Client.ReceiveBufferSize = 128;
                _client.Client.SendBufferSize = 128;
                _client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            }
            catch(Exception e)
            {
                OnError(null, e.Message);
                return false;
            }

            // I fucking hate microsoft please kill yourself
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                const int SIO_UDP_CONNRESET = -1744830452;
                byte[] inValue = new byte[] { 0 };
                byte[] outValue = new byte[] { 0 };
                _client.Client.IOControl(SIO_UDP_CONNRESET, inValue, outValue);
            }

            IsRunning = true;

            _readThread = new(Run);
            _readThread.Name = "UdpServer Worker";
            _readThread.Start();

            OnReady();

            return true;
        }

        public void Send(IPEndPoint endpoint, ref byte[] data) => Send(endpoint, ref data, data.Length);
        public void Send(IPEndPoint endpoint, ref byte[] data, int length)
        {
            _client.Send(data, length, endpoint);
        }

        public void Stop()
        {
            IsRunning = false;
            _readThread!.Join();
        }

        private void Run()
        {
            IPEndPoint? endpoint = new(IPAddress.Any, Port);

            while (IsRunning)
            {
                byte[] data = _client.Receive(ref endpoint);

                if (endpoint == null)
                    continue;
  
                try
                {
                    OnData(endpoint, ref data);
                }
                catch (SocketException ex)
                {
                    OnSocketError(endpoint, ex.SocketErrorCode);
                }
                catch (Exception ex)
                {
                    OnError(endpoint, ex.Message);
                }
            }

            _client.Close();
        }

        protected virtual void OnReady() { }
        protected virtual void OnSocketError(IPEndPoint endpoint, SocketError error) { }
        protected virtual void OnError(IPEndPoint? endpoint, string message) { }
        protected virtual void OnData(IPEndPoint sender, ref byte[] data) { }
    }
}
