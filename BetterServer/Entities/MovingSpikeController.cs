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
    /// <summary>
    /// Moving spike controller
    /// 
    /// Just sends animation frames
    /// </summary>
    internal class MovingSpikeController : Entity
    {
        private int _timer = 2 * Ext.FRAMESPSEC; 
        private int _frame;

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
            if (_timer-- <= 0)
            {
                _frame++;
                if (_frame > 5)
                    _frame = 0;

                if (_frame == 0 || _frame == 2)
                    _timer = 2 * 60;
                else
                    _timer = 0;

                var pk = new TcpPacket(PacketType.SERVER_MOVINGSPIKE_STATE, (byte)_frame);
                server.TCPMulticast(pk);
            }

            return null;
        }
    }
}
