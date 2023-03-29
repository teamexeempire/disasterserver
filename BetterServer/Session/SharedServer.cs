using BetterServer.Data;
using ExeNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace BetterServer.Session
{
    /// <summary>
    /// Server used for important packets (Time sync, entity destruction, etc)
    /// </summary>
    public class SharedServer : TcpServer
    {
        protected Server _server;

        public SharedServer(Server server, int port) : base(IPAddress.Any, port)
        {
            _server = server;
        }

        protected override void OnReady()
        {
            Thread.CurrentThread.Name = $"Server {_server.UID}";
            Logger.LogDiscord($" TCP port {Port}");

            base.OnReady();
        }

        protected override void OnSocketError(SocketError error)
        {
            Thread.CurrentThread.Name = $"Server {_server.UID}";
            Logger.LogDiscord($"Caught SocketError: {error}");

            base.OnSocketError(error);
        }

        protected override void OnError(string message)
        {
            Thread.CurrentThread.Name = $"Server {_server.UID}";
            Logger.LogDiscord($"Caught Exception: {message}");

            base.OnError(message);
        }

        protected override TcpSession CreateSession(TcpClient client)
        {
            return new SharedServerSession(_server, client);
        }
    }
}
