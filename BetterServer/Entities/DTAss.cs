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
    public class DTAss : Entity
    {
        public static byte SID = 0;
        public byte ID = 0;

        private bool _state = false;
        private double _accel = 0;

        private double _y = 0;
        private int _sY = 0;
        private int _timer = -Ext.FRAMESPSEC;

        public DTAss(int x, int y)
        {
            X = x;
            Y = y;
            _y = y;
            _sY = y;

            ID = SID++;
        }

        public override TcpPacket? Destroy(Server server, Game game, Map map)
        {
            return null;
        }

        public override TcpPacket? Spawn(Server server, Game game, Map map)
        {
            return new TcpPacket(PacketType.SERVER_DTASS_STATE, (byte)0, (byte)ID, (ushort)X, (ushort)Y);
        }

        public override UdpPacket? Tick(Server server, Game game, Map map)
        {
            if (_timer == 0)
            {
                server.TCPMulticast(new TcpPacket(PacketType.SERVER_DTASS_STATE, (byte)0, (byte)ID, (ushort)X, (ushort)Y));
            }

            if (_timer > -Ext.FRAMESPSEC)
                _timer--;

            if (_timer <= -Ext.FRAMESPSEC && !_state)
            {
                lock (server.Peers)
                {
                    foreach (var peer in server.Peers)
                    {
                        if (peer.Value.Player.Invisible)
                            continue;

                        var dist = peer.Value.Player.Y - Y;

                        if (dist > 0 && dist <= 336 && peer.Value.Player.X >= X && peer.Value.Player.X <= X + 80)
                        {
                            server.TCPMulticast(new TcpPacket(PacketType.SERVER_DTASS_STATE, (byte)2, (byte)ID));
                            _state = true;
                            break;
                        }
                    }
                }
            }

            if (_state)
            {
                _accel += 0.164;
                _y += _accel;

                Y = (int)_y;
                return new UdpPacket(PacketType.SERVER_DTASS_STATE, ID, (ushort)X, (ushort)Y);
            }

            return null;
        }

        public void Dectivate(Server server)
        {
            _state = false;
            _y = _sY;
            Y = _sY;
            _timer = Ext.FRAMESPSEC * 25;
            _accel = 0;

            server.TCPMulticast(new TcpPacket(PacketType.SERVER_DTASS_STATE, (byte)1, (byte)ID));
        }
    }
}
