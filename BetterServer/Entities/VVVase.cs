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
    public class VVVase : Entity
    {
        public byte ID;
        public byte Type = (byte)_rand.Next(0, 4);
        public ushort DestroyerID;

        private static readonly Random _rand = new();

        public VVVase(byte id)
        {
            ID = id;
        }

        public override TcpPacket? Destroy(Server server, Game game, Map map)
        {
            return new TcpPacket(PacketType.SERVER_VVVASE_STATE, ID, Type, DestroyerID);
        }

        public override TcpPacket? Spawn(Server server, Game game, Map map)
        {
            return null;
        }

        public override UdpPacket? Tick(Server server, Game game, Map map)
        {
            return null;
        }
    }
}
