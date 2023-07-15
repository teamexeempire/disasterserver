using BetterServer.Maps;
using BetterServer.Session;
using BetterServer.State;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetterServer.Entities
{
    public class Fart : Entity
    {
        private float _xspd = 0;
        private float _x = 0;

        public override TcpPacket? Destroy(Server server, Game game, Map map)
        {
            return null;
        }

        public override TcpPacket? Spawn(Server server, Game game, Map map)
        {
            X = 1616;
            Y = 2608;
            _x = X;

            return null;
        }

        public override UdpPacket? Tick(Server server, Game game, Map map)
        {
            _x += _xspd;
            _x = Math.Clamp(_x, 1282, 2944);

            _xspd -= MathF.Min(MathF.Abs(_xspd), 0.046875f * 4f) * MathF.Sign(_xspd);
            return new UdpPacket(PacketType.SERVER_FART_STATE, (ushort)_x, (ushort)Y);
        }

        public void Push(sbyte force)
        {
            _xspd = force;
        }
    }
}
