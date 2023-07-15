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
    internal class KAFSpeedBooster : Entity
    {
        public byte ID = 0;
        private int _timer = 0;
        private bool _activated = false;

        public override TcpPacket? Spawn(Server server, Game game, Map map)
        {
            return new TcpPacket(PacketType.SERVER_KAFMONITOR_STATE, (byte)0, ID);
        }

        public override TcpPacket? Destroy(Server server, Game game, Map map)
        {
            return null;
        }

        public override UdpPacket? Tick(Server server, Game game, Map map)
        {
            if (_timer > 0)
                _timer--;
            else if (_timer == 0)
            {
                _activated = false;

                var pack = new TcpPacket(PacketType.SERVER_KAFMONITOR_STATE);
                pack.Write((byte)1);
                pack.Write(ID);
                server.TCPMulticast(pack);
                _timer = -1;
            }

            return null;
        }

        public void Activate(Server server, ushort nid, bool isProjectile)
        {
            if(_activated)
                return;

            var pack = new TcpPacket(PacketType.SERVER_KAFMONITOR_STATE);
            pack.Write((byte)2);
            pack.Write(ID);
            pack.Write(isProjectile ? 0 : nid);
            server.TCPMulticast(pack);

            _activated = true;
            _timer = 30 * 60;
        }
    }
}
