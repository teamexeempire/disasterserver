using BetterServer.Maps;
using BetterServer.Session;
using BetterServer.State;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetterServer.Entities
{
    public abstract class Entity
    {
        public int X, Y;
        public abstract TcpPacket? Spawn(Server server, Game game, Map map);
        public abstract UdpPacket? Tick(Server server, Game game, Map map);
        public abstract TcpPacket? Destroy(Server server, Game game, Map map);
    }
}
