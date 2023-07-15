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
    public class VVLava : Entity
    {
        public float StartY;
        public float TravelDistance;
        public byte ID;

        private byte _state;
        private int _timer = 20 * Ext.FRAMESPSEC;
        private float _y;
        private float _accel;

        private static Random _rand = new();

        public VVLava(byte id, float startY, float dist)
        {
            ID = id;
            StartY = startY;
            _y = startY;
            TravelDistance = dist;
        }

        public override TcpPacket? Destroy(Server server, Game game, Map map)
        {
            return null;
        }

        public override TcpPacket? Spawn(Server server, Game game, Map map)
        {
            _timer += _rand.Next(2, 5) * Ext.FRAMESPSEC;
            return null;
        }

        public override UdpPacket? Tick(Server server, Game game, Map map)
        {
            switch (_state)
            {
                // Just move usin sin
                case 0:
                    _y = StartY + MathF.Sin(_timer / 25.0f) * 6;

                    if(_timer-- <= 0)
                        _state = 1;

                    break;

                // Move down a bit
                case 1:

                    if (_y < StartY + 20)
                    {
                        _y += 0.15f;
                    }
                    else
                        _state = 2;

                    break;

                // Start growth
                case 2:
                    if (_y > StartY - TravelDistance)
                    {
                        if (_accel < 5)
                            _accel += 0.08f;
                        
                        _y -= _accel;
                    }
                    else
                    {
                        _state = 3;
                        _timer = Ext.FRAMESPSEC * 5;
                        _accel = 0;
                    }
                    break;

                case 3:
                    if (_timer-- <= 0)
                        _state = 4;

                    _y = (StartY - TravelDistance) + MathF.Sin(_timer / 25.0f) * 6;
                    break;

                case 4:
                    if(StartY > _y)
                    {
                        if (_accel < 5)
                            _accel += 0.08f;

                        _y += _accel;
                    }
                    else
                    {
                        _state = 0;
                        _timer = Ext.FRAMESPSEC * 20;
                        _accel = 0;
                    }
                    break;
            }

            return new UdpPacket(PacketType.SERVER_VVLCOLUMN_STATE, (byte)ID, (byte)_state, (float)_y);
        }
    }
}
