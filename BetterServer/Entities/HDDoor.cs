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
    public class HDDoor : Entity
    {
        public bool _state = false;
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
            if(_timer > 0)
            {
                _timer--;
                if(_timer == 0)
                    server.TCPMulticast(new TcpPacket(PacketType.SERVER_HDDOOR_STATE, (byte)1, true));
            }
            return null;
        }

        public void Toggle(Server server)
        {
            if (_timer > 0)
                return;

            _state = !_state;
            _timer = Ext.FRAMESPSEC * 10;

            server.TCPMulticast(new TcpPacket(PacketType.SERVER_HDDOOR_STATE, (byte)0, _state));
            server.TCPMulticast(new TcpPacket(PacketType.SERVER_HDDOOR_STATE, (byte)1, false));
        }
    }
}
