using BetterServer.Maps;
using BetterServer.Session;
using BetterServer.State;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace BetterServer.Entities
{
    public class CreamRing : Ring
    {
        public override TcpPacket? Spawn(Server server, Game game, Map map)
        {
            IID = 255;
            ID = map.RingIDs++;

            return new TcpPacket
            (
                PacketType.SERVER_RING_STATE,

                (byte)2,
                (ushort)X,
                (ushort)Y,
                IID,
                ID,
                IsRedRing
            );
        }

        public override TcpPacket? Destroy(Server server, Game game, Map map)
        {
            return new TcpPacket
            (
                PacketType.SERVER_RING_STATE,

                (byte)1,
                IID,
                ID
            );
        }

        public override UdpPacket? Tick(Server server, Game game, Map map)
        {
            return null;
        }
    }
}
