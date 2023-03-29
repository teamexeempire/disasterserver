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
    internal class YCRSmokeController : Entity
    {
        public int _timer = 0;
        public bool _activated = false;
        public byte _id = 0;

        private Random _rand = new();

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
            if (_timer > 60 * 10)
            {
                _timer = 0;
                _activated = !_activated;

                if (_activated)
                    _id = (byte)_rand.Next(7);
                else
                    _id = 0;

                Logger.Log($"{_id}: state {_activated}");
                var packet = new TcpPacket(PacketType.SERVER_YCRSMOKE_STATE);
                packet.Write(_activated);
                packet.Write(_id);
                server.TCPMulticast(packet);
            }

            if (_activated && _timer == 60 * 2)
            {
                var packet = new TcpPacket(PacketType.SERVER_YCRSMOKE_READY);
                packet.Write(_id);
                server.TCPMulticast(packet);
            }

            _timer++;
            return null;
        }
    }
}
