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
    public class DarkTower : Map
    {
        public override void Init(Server server)
        {
            Spawn<DTTailsDoll>(server);
            Spawn<DTBall>(server);

            Spawn(server, new DTAss(1744, 224));
            Spawn(server, new DTAss(1840, 224));
            Spawn(server, new DTAss(1936, 224));
            Spawn(server, new DTAss(2032, 224));
            Spawn(server, new DTAss(2128, 224));

            Spawn(server, new DTAss(1824, 784));
            Spawn(server, new DTAss(1920, 784));
            Spawn(server, new DTAss(2016, 784));
            Spawn(server, new DTAss(2112, 784));
            Spawn(server, new DTAss(2208, 784));

            Spawn(server, new DTAss(2464, 1384));
            Spawn(server, new DTAss(2592, 1384));

            Spawn(server, new DTAss(3032, 64));
            Spawn(server, new DTAss(3088, 64));

            SetTime(server, 205);
            base.Init(server);
        }

        protected override int GetRingSpawnCount()
        {
            return 31;
        }

        public override void PeerTCPMessage(Server server, TcpSession session, BinaryReader reader)
        {
            var passtrough = reader.ReadBoolean();
            var type = reader.ReadByte();

            switch ((PacketType)type)
            {
                case PacketType.CLIENT_DTASS_ACTIVATE:
                    {
                        var id = reader.ReadByte();

                        var list = FindOfType<DTAss>();
                        if (list == null)
                            break;

                        var baller = list.FirstOrDefault(e => e.ID == id);
                        if (baller == null)
                            break;

                        baller.Dectivate(server);
                        break;
                    }
            }

            base.PeerTCPMessage(server, session, reader);
        }
    }
}
