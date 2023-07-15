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
    internal class Act9Wall : Entity
    {
        private byte _id = 0;
        private ushort _tx = 0;
        private ushort _ty = 0;
        private int _startTime;

        public Act9Wall(byte id, ushort tx, ushort ty)
        {
            _id = id;
            _tx = tx;
            _ty = ty;
        }

        public override TcpPacket? Destroy(Server server, Game game, Map map)
        {
            return null;
        }

        public override TcpPacket? Spawn(Server server, Game game, Map map)
        {
            _startTime = map.Timer;
            return null;
        }

        public override UdpPacket? Tick(Server server, Game game, Map map)
        {
            X = (int)(_tx * ((_startTime - map.Timer) / (double)_startTime));
            Y = (int)(_ty * ((_startTime - map.Timer) / (double)_startTime));

            return new UdpPacket(PacketType.SERVER_ACT9WALL_STATE, _id, (ushort)X, (ushort)Y);
        }
    }
}
