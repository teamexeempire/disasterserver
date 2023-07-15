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
    public class NastyParadise : Map
    {
        private int _timer = 0;

        public override void Init(Server server)
        {
            for(byte i = 0; i < 10; i++)
                Spawn(server, new NAPIce(i));

            var ball = Spawn(server, new NAPSnowball(0, 10, 1));

            for (byte i = 0; i < 4; i++)
            {
                ball.SetWaypointMoveSpeed((byte)(5 + i), 0.05f + 0.05f * (i / 4.0f));
                ball.SetWaypointAnimSpeed((byte)(5 + i), 0.35f + 0.25f * (i / 4.0f));
            }

            ball = Spawn(server, new NAPSnowball(1, 8, -1));

            for (byte i = 0; i < 5; i++)
            {
                ball.SetWaypointMoveSpeed((byte)(2 + i), 0.05f + 0.05f * (i / 5.0f));
                ball.SetWaypointAnimSpeed((byte)(2 + i), 0.35f + 0.25f * (i / 5.0f));
            }

            ball = Spawn(server, new NAPSnowball(2, 11, 1));

            for (byte i = 0; i < 5; i++)
            {
                ball.SetWaypointMoveSpeed((byte)(5 + i), 0.05f + 0.05f * (i / 5.0f));
                ball.SetWaypointAnimSpeed((byte)(5 + i), 0.35f + 0.25f * (i / 5.0f));
            }

            ball = Spawn(server, new NAPSnowball(3, 9, 1));

            for (byte i = 0; i < 2; i++)
            {
                ball.SetWaypointMoveSpeed((byte)(6 + i), 0.05f + 0.05f * (i / 2.0f));
                ball.SetWaypointAnimSpeed((byte)(6 + i), 0.35f + 0.25f * (i / 2.0f));
            }

            Spawn(server, new NAPSnowball(4, 5, -1));

            SetTime(server, 155);
            base.Init(server);
        }

        public override void Tick(Server server)
        {
            _timer++;
            if (_timer >= Ext.FRAMESPSEC * 20)
            {
                _timer = 0;

                var ents = FindOfType<NAPSnowball>();

                foreach (var ent in ents)
                    ent.Activate(server);
            }

            base.Tick(server);
        }

        public override void PeerTCPMessage(Server server, TcpSession session, BinaryReader reader)
        {
            var passtrough = reader.ReadBoolean();
            var type = reader.ReadByte();

            switch ((PacketType)type)
            {
                case PacketType.CLIENT_NAPICE_ACTIVATE:
                    {
                        var id = reader.ReadByte();
                        var ices = FindOfType<NAPIce>();

                        if (ices == null)
                            break;

                        var ice = ices.Where(e => e.ID == id).FirstOrDefault();

                        if (ice == null)
                            break;

                        ice.Activate(server);
                        break;
                    }
            }
            base.PeerTCPMessage(server, session, reader);
        }

        protected override int GetRingSpawnCount()
        {
            return 26;
        }
    }
}
