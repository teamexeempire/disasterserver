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
    public class NAPIce : Entity
    {
        public byte ID = 0;

        private bool _activated = true;
        private int _timer = 0;

        public NAPIce(byte id)
        {
            ID = id;
        }

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
            if (!_activated)
                return null;

            if (_timer-- <= 0)
            {
                _activated = false;
                _timer = Ext.FRAMESPSEC * 15;

                server.TCPMulticast(new TcpPacket
                (
                    PacketType.SERVER_NAPICE_STATE,
                    (byte)1,
                    (byte)ID
                ));
            }

            return null;
        }

        public void Activate(Server server)
        {
            if (_activated)
                return;

            _timer = Ext.FRAMESPSEC * 15;
            _activated = true;

            server.TCPMulticast(new TcpPacket
            (
                PacketType.SERVER_NAPICE_STATE,
                (byte)0,
                (byte)ID
            ));
        }
    }
}
