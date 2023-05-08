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
    public class RMZShard : Entity
    {
        private static byte GID = 0;

        public byte ID = GID++;
        private bool _isSpawned = false;

        public RMZShard(int x, int y, bool spawned = false)
        {
            X = x;
            Y = y;
            _isSpawned = spawned;
        }

        public override TcpPacket? Destroy(Server server, Game game, Map map)
        {
            return null;
        }

        public override TcpPacket? Spawn(Server server, Game game, Map map)
        {
            return new TcpPacket(PacketType.SERVER_RMZSHARD_STATE, (byte)(_isSpawned ? 1 : 0), (byte)ID, (ushort)X, (ushort)Y);
        }

        public override UdpPacket? Tick(Server server, Game game, Map map)
        {
            return null;
        }
    }
}
