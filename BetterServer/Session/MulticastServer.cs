using ExeNet;
using System.Net;
using System.Net.Sockets;

namespace BetterServer.Session
{
    /// <summary>
    /// Server used for unimportant packets (Player data)
    /// </summary>
    public class MulticastServer : UdpServer
    {
        protected Server _server;

        public MulticastServer(Server server, int port) : base(port)
        {
            _server = server;
        }

        protected override void OnReady()
        {
            Thread.CurrentThread.Name = $"Server {_server.UID}";
            Terminal.LogDiscord($"Server started on UDP port {Port}");

            base.OnReady();
        }

        protected override void OnSocketError(IPEndPoint endpoint, SocketError error)
        {
            Thread.CurrentThread.Name = $"Server {_server.UID}";
            Terminal.LogDiscord($"Caught Error: {error}");

            _server.State.UDPSocketError(endpoint, error);

            base.OnSocketError(endpoint, error);
        }
        protected override void OnError(IPEndPoint? endpoint, string message)
        {
            Thread.CurrentThread.Name = $"Server {_server.UID}";
            Terminal.LogDiscord($"Caught Error: {message}");

            base.OnError(endpoint, message);
        }

        protected override void OnData(IPEndPoint sender, byte[] data)
        {
            Thread.CurrentThread.Name = $"Server {_server.UID}";

            if (data.Length > 128)
            {
                Terminal.LogDiscord("UDP overload (data.Length > 128)");
                return;
            }
            
            using var stream = new MemoryStream(data, 0, data.Length, false);
            using var reader = new BinaryReader(stream);
            _server.State.PeerUDPMessage(_server, sender, reader);

            base.OnData(sender, data);
        }
    }
}
