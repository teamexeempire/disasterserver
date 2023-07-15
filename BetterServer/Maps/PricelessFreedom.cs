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
    public class PricelessFreedom : Map
    {
        public override void Init(Server server)
        {
            SetTime(server, 155);
            Spawn(server, new PFLift(0, 1669, 1016));
            Spawn(server, new PFLift(1, 1069, 704));
            Spawn(server, new PFLift(2, 829, 400));
            Spawn(server, new PFLift(3, 1070, 544));

            for (var i = 0; i < 29; i++)
                Spawn<BlackRing>(server);

            base.Init(server);
        }

        public override void PeerTCPMessage(Server server, TcpSession session, BinaryReader reader)
        {
            var passtrough = reader.ReadBoolean();
            var type = reader.ReadByte();

            switch ((PacketType)type)
            {
                case PacketType.CLIENT_PFLIT_ACTIVATE:
                    {
                        var id = reader.ReadByte();
                        var lifts = FindOfType<PFLift>();

                        if (lifts == null)
                            break;

                        var lift = lifts.Where(e => e.ID == id).FirstOrDefault();

                        if (lift == null)
                            break;

                        lift.Activate(server, session.ID);
                        break;
                    }
            }
            base.PeerTCPMessage(server, session, reader);
        }

        protected override int GetPlayerOffset(Server server)
        {
            lock (server.Peers)
                return (server.Peers.Count - 1) * 10;
        }

        protected override int GetRingSpawnCount()
        {
            return 38;
        }
    }
}
