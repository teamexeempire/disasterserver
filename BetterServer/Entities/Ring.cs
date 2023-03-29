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
    public class Ring : Entity
    {
        public ushort ID = 0;
        public byte IID = 0;
        public bool IsRedRing = false;

        private static readonly Random _rand = new();

        public override TcpPacket? Spawn(Server server, Game game, Map map)
        {
            ID = map.RingIDs++;
            IID = (byte)_rand.Next(255);
            IsRedRing = _rand.Next(100) <= 10;

            return new TcpPacket
            (
                PacketType.SERVER_RING_STATE,

                (byte)0,
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
