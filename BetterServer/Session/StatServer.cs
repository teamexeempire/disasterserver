using ExeNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Timer = System.Timers.Timer;

namespace BetterServer.Session
{
    public class StatServer : TcpServer
    {
        public StatServer() : base(IPAddress.Any, 12084)
        {
        }
        
        public void MulticastInformation()
        {
            try
            {
                byte[] arr = new byte[64];
                arr[0] = (byte)Program.Servers.Count;
                arr[1] = 7;

                int ind = 2;
                foreach (var server in Program.Servers)
                {
                    lock (server.Peers)
                    {
                        byte state = (byte)server.State.AsState();
                        byte players = (byte)server.Peers.Count;

                        arr[ind++] = state;
                        arr[ind++] = players;
                    }
                }

                lock (Sessions)
                {
                    foreach (var session in Sessions)
                        session.Send(arr);
                }
            }
            catch
            {
            }
        }

        protected override TcpSession CreateSession(TcpClient client)
        {
            return new StatServerSession(this, client);
        }
    }
}
