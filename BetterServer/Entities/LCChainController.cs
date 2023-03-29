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
    internal class LCChainController : Entity
    {
        private int _timer = 0;

        public override TcpPacket? Spawn(Server server, Game game, Map map)
        {
            return null;
        }

        public override TcpPacket? Destroy(Server server, Game game, Map map)
        {
            return null;
        }

        public override UdpPacket? Tick(Server server, Game game, Map map)
        {
            if (_timer == 60 * 8)
            {
                var packet = new TcpPacket(PacketType.SERVER_LCCHAIN_STATE);
                packet.Write(0);
                server.TCPMulticast(packet);
            }

            if (_timer == 60 * 10)
            {
                var packet = new TcpPacket(PacketType.SERVER_LCCHAIN_STATE);
                packet.Write(1);
                server.TCPMulticast(packet);
            }

            if (_timer >= 60 * 12)
            {
                var packet = new TcpPacket(PacketType.SERVER_LCCHAIN_STATE);
                packet.Write(2);
                server.TCPMulticast(packet);

                _timer = 0;
            }

            _timer++;
            return null;
        }
    }
}
