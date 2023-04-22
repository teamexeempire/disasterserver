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
    internal class NotPerfectController : Entity
    {
        private byte _stage = 0;
        private int _timer = 0;

        public override TcpPacket? Destroy(Server server, Game game, Map map)
        {
            return null;
        }

        public override TcpPacket? Spawn(Server server, Game game, Map map)
        {
            return null;
        }

        public override UdpPacket? Tick(Server server, Game game, Map map)
        {
            if (map.BigRingSpawned)
            {
                if (_timer == 2 * 60)
                {
                    var pack = new TcpPacket(PacketType.SERVER_NPCONTROLLER_STATE);
                    pack.Write(false);
                    pack.Write((byte)0);
                    pack.Write((byte)0);
                    server.TCPMulticast(pack);
                }

                if (_timer >= 5 * 60)
                {
                    _stage++;
                    _timer = 0;

                    var pack = new TcpPacket(PacketType.SERVER_NPCONTROLLER_STATE);
                    pack.Write(true);
                    pack.Write((byte)(_stage % 4));
                    pack.Write((byte)((Math.Max(_stage - 1, 0)) % 4));
                    server.TCPMulticast(pack);
                }
            }
            else
            {

                if (_timer == 60 * 15)
                {
                    var pack = new TcpPacket(PacketType.SERVER_NPCONTROLLER_STATE);
                    pack.Write(false);
                    pack.Write((byte)0);
                    pack.Write((byte)0);
                    server.TCPMulticast(pack);
                }

                if (_timer >= 60 * 20)
                {
                    _stage++;
                    _timer = 0;

                    var pack = new TcpPacket(PacketType.SERVER_NPCONTROLLER_STATE);
                    pack.Write(true);
                    pack.Write((byte)(_stage % 4));
                    pack.Write((byte)((Math.Max(_stage - 1, 0)) % 4));
                    server.TCPMulticast(pack);
                }
            }

            _timer++;
            return null;
        }
    }
}
