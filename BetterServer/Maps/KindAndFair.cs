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
    public class KindAndFair : Map
    {
        public override void Init(Server server)
        {
            for (var i = 0; i < 11; i++)
            {
                Spawn(server, new KAFSpeedBooster()
                {
                    ID = (byte)i
                });
            }

            SetTime(server, 180);
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
                case PacketType.CLIENT_KAFMONITOR_ACTIVATE:
                    {
                        var nid = reader.ReadByte();
                        var isProj = reader.ReadBoolean();

                        var list = FindOfType<KAFSpeedBooster>();

                        if (list == null)
                            return;

                        if (nid >= list.Length)
                            break;

                        var act = list[nid];
                        lock (server.Peers)
                            act.Activate(server, server.Peers[session.ID].ID, isProj);

                        break;
                    }
            }

            base.PeerTCPMessage(server, session, reader);
        }

        protected override int GetRingSpawnCount()
        {
            return 31;
        }
    }
}
