using BetterServer.Maps;
using BetterServer.Session;
using BetterServer.State;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace BetterServer.Entities
{
    public class BlackRing : Entity
    {
        public ushort ID = 1;

        public override TcpPacket? Destroy(Server server, Game game, Map map)
        {
            return new TcpPacket
            (
                PacketType.SERVER_BRING_STATE,

                (byte)1,
                ID
            );
        }

        public override TcpPacket? Spawn(Server server, Game game, Map map)
        {
            ID = map.BRingsIDs++;

            return new TcpPacket
            (
                PacketType.SERVER_BRING_STATE,

                (byte)0,
                ID
            );
        }

        public override UdpPacket? Tick(Server server, Game game, Map map)
        {
            return null;
        }
    }
}
