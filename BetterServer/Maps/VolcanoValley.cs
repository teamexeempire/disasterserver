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
    public class VolcanoValley : Map
    {
        public override void Init(Server server)
        {
            SetTime(server, 180);

            Spawn(server, new VVLava(0, 736, 130));
            Spawn(server, new VVLava(1, 1388, 130));
            Spawn(server, new VVLava(2, 1524, 130));
            Spawn(server, new VVLava(3, 1084, 130));

            for (byte i = 0; i < 14; i++)
                Spawn(server, new VVVase(i));

            base.Init(server);
        }

        public override void PeerTCPMessage(Server server, TcpSession session, BinaryReader reader)
        {
            var passtrough = reader.ReadBoolean();
            var type = reader.ReadByte();

            switch ((PacketType)type)
            {
                case PacketType.CLIENT_VVVASE_BREAK:
                    {
                        var nid = reader.ReadByte();
                        var list = FindOfType<VVVase>();

                        if (list == null)
                            return;

                        var vase = list.Where(e => e.ID == nid).FirstOrDefault();
                        if (vase == null)
                            break;

                        vase.DestroyerID = session.ID;
                        Destroy(server, vase);
                        break;
                    }
            }

            base.PeerTCPMessage(server, session, reader);
        }

        protected override int GetRingSpawnCount()
        {
            return 27;
        }
    }
}
