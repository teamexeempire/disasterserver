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
    internal class TCGom : Entity
    {
        private int _timer = 3 * Ext.FRAMESPSEC;
        private int _id = 0;
        private bool _state = false;
        private Random _rand = new();

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
            if(_timer-- <= 0)
            {
                _timer = 3 * Ext.FRAMESPSEC;
                _state = !_state;

                if(_state)
                    _id = _rand.Next(7);

                server.TCPMulticast(new TcpPacket(PacketType.SERVER_TCGOM_STATE, (byte)_id, _state));
            }

            return null;
        }
    }
}
