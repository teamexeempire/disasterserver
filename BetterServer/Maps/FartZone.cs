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
    internal class FartZone : Map
    {
        public override void Init(Server server)
        {
            SetTime(server, 256);

            Spawn<Fart>(server);
            Spawn<MovingSpikeController>(server);
            Spawn<DTBall>(server);

            for (var i = 0; i < 3; i++)
                Spawn<BlackRing>(server);

            base.Init(server);
        }

        public override void PeerTCPMessage(Server server, TcpSession session, BinaryReader reader)
        {
            var passtrough = reader.ReadBoolean();
            var type = reader.ReadByte();

            switch ((PacketType)type)
            {
                case PacketType.CLIENT_FART_PUSH:
                    {
                        var spd = reader.ReadSByte();
                        var list = FindOfType<Fart>();

                        if (list == null)
                            return;

                        var act = list[0];
                        act.Push(spd);

                        break;
                    }
            }

            base.PeerTCPMessage(server, session, reader);
        }

        protected override int GetRingSpawnCount()
        {
            return 15;
        }

        protected override float GetRingTime()
        {
            return 1;
        }
    }
}