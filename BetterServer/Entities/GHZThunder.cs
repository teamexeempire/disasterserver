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
    public class GHZThunder : Entity
    {
        private int _timer = 0;
        private Random _rand = new();

        public override TcpPacket? Destroy(Server server, Game game, Map map)
        {
            return null;
        }

        public override TcpPacket? Spawn(Server server, Game game, Map map)
        {
            _timer = Ext.FRAMESPSEC * _rand.Next(15, 20);
            return null;
        }

        public override UdpPacket? Tick(Server server, Game game, Map map)
        {
            if(_timer == Ext.FRAMESPSEC * 2)
            {
                var pack = new TcpPacket(PacketType.SERVER_GHZTHUNDER_STATE, (byte)0);
                server.TCPMulticast(pack);
            }
            
            if(_timer <= 0)
            {
                _timer = Ext.FRAMESPSEC * _rand.Next(15, 20);
                var pack = new TcpPacket(PacketType.SERVER_GHZTHUNDER_STATE, (byte)1);
                server.TCPMulticast(pack);
            }

            _timer--;
            return null;
        }
    }
}
