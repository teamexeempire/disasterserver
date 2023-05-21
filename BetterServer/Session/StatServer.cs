using ExeNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace BetterServer.Session
{
    internal class StatServer : TcpServer
    {
        public StatServer() : base(IPAddress.Any, 12084)
        {
        }

        protected override TcpSession CreateSession(TcpClient client)
        {
            return new StatServerSession(this, client);
        }
    }
}
