using BetterServer.Data;
using BetterServer.Entities;
using BetterServer.Session;
using ExeNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetterServer.Maps
{
    public class RavineMist : Map
    {
        private Vector2[] SpawnPoints = new Vector2[]
        {
            new Vector2(192, 632),
            new Vector2(1704, 728),
            new Vector2(896, 424),
            new Vector2(208, 1672),
            new Vector2(904, 1263),
            new Vector2(184, 631),
            new Vector2(1672, 727),
        };

        public override void Init(Server server)
        {
            foreach (var coord in SpawnPoints)
                Spawn(server, new RMZSlimeSpawner(coord.X, coord.Y));

            SetTimer(server, 180);
            base.Init(server);
        }

        public override void Tick(Server server)
        {
            base.Tick(server);
        }

        public override void PeerTCPMessage(Server server, TcpSession session, BinaryReader reader)
        {
            var passtrough = reader.ReadBoolean();
            var type = reader.ReadByte();

            switch((PacketType)type)
            {
                case PacketType.CLIENT_RMZSLIME_HIT:
                    {
                        lock (Entities)
                        {
                            var id = reader.ReadByte();
                            var pid = reader.ReadUInt16();
                            var isProj = reader.ReadBoolean();

                            Logger.Log($"Killing slime {id}");

                            var slimes = FindOfType<RMZSlimeSpawner>();
                            if (slimes == null)
                                break;

                            var slime = slimes.Where(e => e.ID == id).FirstOrDefault();
                            if (slime == null)
                                break;

                            Peer? killer;
                            lock(server.Peers)
                                killer = server.Peers.Values.Where(e => e.ID == pid).FirstOrDefault();

                            if (killer == null)
                                break;

                            slime.KillSlime(server, this, killer, isProj);
                            Logger.Log($"Killed slime {id}");
                            break;
                        }
                    }
            }

            base.PeerTCPMessage(server, session, reader);
        }
    }
}
