using System;
using System.Collections.Generic;
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

        public void Send(IPEndPoint endpoint, byte[] data) => Send(endpoint, data, data.Length);
        public void Send(IPEndPoint endpoint, byte[] data, int length)
        {
            _client.BeginSend(data, length, endpoint, new AsyncCallback(DoSend), endpoint);
        }

        public void Stop()
        {
            IsRunning = false;
            _readThread!.Join();
        }

        private void Run()
        {
            IPEndPoint endpoint = new(IPAddress.Any, Port);
            _client.BeginReceive(new AsyncCallback(DoReceive), endpoint);

            while (IsRunning)
                Thread.Sleep(100);

            _client.Close();
        }

        private void DoReceive(IAsyncResult? result)
        {
            if (result == null) 
                return;

            IPEndPoint? endPoint = (IPEndPoint?)result.AsyncState;

            if (endPoint == null)
                return;

            try
            {
                byte[] bytes = _client.EndReceive(result, ref endPoint);
                _client.BeginReceive(new AsyncCallback(DoReceive), endPoint);

                OnData(endPoint, bytes);
            }
            catch (SocketException ex)
            {
                OnSocketError(endPoint, ex.SocketErrorCode);
                _client.BeginReceive(new AsyncCallback(DoReceive), endPoint);
            }
            catch (Exception ex)
            {
                OnError(endPoint, ex.Message);
                _client.BeginReceive(new AsyncCallback(DoReceive), endPoint);
            }
        }
        private void DoSend(IAsyncResult? result)
        {
            IPEndPoint? endpoint = (IPEndPoint?)result.AsyncState;

            if (endpoint == null)
                return;

            try
            {
                int cnt = _client.EndSend(result);
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

        protected virtual void OnReady() { }
        protected virtual void OnSocketError(IPEndPoint endpoint, SocketError error) { }
        protected virtual void OnError(IPEndPoint? endpoint, string message) { }
        protected virtual void OnData(IPEndPoint sender, byte[] data) { }
    }
}
