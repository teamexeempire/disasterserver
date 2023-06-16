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
    public class DTBall : Entity
    {
        private double _state = 0;
        private bool _side = true;

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
            if (_side)
            {
                _state += 0.015f;

                if (_state >= 1)
                    _side = false;
            }
            else
            {
                _state -= 0.015f;

                if (_state <= -1)
                    _side = true;
            }

            return new UdpPacket(PacketType.SERVER_DTBALL_STATE, (float)_state);
        }
    }
}
