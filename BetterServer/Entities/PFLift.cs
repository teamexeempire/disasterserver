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
    internal class PFLift : Entity
    {
        public byte ID;
        public ushort ActivatorID;

        private bool _activated;
        private float _y, _startY, _endY;
        private int _timer;
        private float _speed = 0;
        
        public PFLift(byte id, float starty, float endY)
        {
            ID = id;
            _startY = starty;
            _endY = endY;
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
            {
                if (_timer > 0)
                {
                    _timer--;
                    
                    if(_timer == 0)
                        server.TCPMulticast(new TcpPacket(PacketType.SERVER_PFLIFT_STATE, (byte)3, (byte)ID, (ushort)_startY));
                }

                return null;
            }

            if (_y > _endY)
            {
                if (_speed < 7f)
                    _speed += 0.052f;

                _y -= _speed;
            }
            else
            {
                server.TCPMulticast(new TcpPacket(PacketType.SERVER_PFLIFT_STATE, (byte)2, (byte)ID, (ushort)ActivatorID, (ushort)_y));
                _timer = (int)(1.5 * Ext.FRAMESPSEC);
                _activated = false;
                ActivatorID = 0;
            }

            return new UdpPacket(PacketType.SERVER_PFLIFT_STATE, (byte)1, (byte)ID, (ushort)ActivatorID, (ushort)_y);
        }

        public void Activate(Server server, ushort id)
        {
            if (_activated)
                return;

            if (_timer > 0)
                return;

            ActivatorID = id;
            _timer = 0;
            _speed = 0;
            _y = _startY;
            _activated = true;

            server.TCPMulticast(new TcpPacket(PacketType.SERVER_PFLIFT_STATE, (byte)0, (byte)ID, (ushort)ActivatorID));
        }
    }
}
