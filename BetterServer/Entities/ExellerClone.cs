using BetterServer.Data;
using BetterServer.Maps;
using BetterServer.Session;
using BetterServer.State;
using ExeNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetterServer.Entities
{
    public class ExellerClone : Entity
    {
        public byte ID = 0;
        public ushort PID = 0;
        public sbyte Dir = 0;

        public ExellerClone(ushort pid, byte id, int x, int y, sbyte dir)
        {
            Dir = dir;
            PID = pid;
            ID = id;
            X = x;
            Y = y;
        }

        public override TcpPacket? Destroy(Server server, Game game, Map map)
        {
            return new TcpPacket(PacketType.SERVER_EXELLERCLONE_STATE, (byte)1, ID);
        }

        public override TcpPacket? Spawn(Server server, Game game, Map map)
        {
            return new TcpPacket(PacketType.SERVER_EXELLERCLONE_STATE, (byte)0, ID, PID, (ushort)X, (ushort)Y, (sbyte)Dir);
        }

        public override UdpPacket? Tick(Server server, Game game, Map map)
        {
            return null;
        }
    }
}
