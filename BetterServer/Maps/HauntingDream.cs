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
    public class HauntingDream : Map
    {
        public override void Init(Server server)
        {
            Spawn<HDDoor>(server);
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
                case PacketType.CLIENT_HDDOOR_TOGGLE:
                    {
                        var list = FindOfType<HDDoor>();
                        if (list == null)
                            break;

                        var baller = list.FirstOrDefault();
                        if (baller == null)
                            break;

                        baller.Toggle(server);
                        break;
                    }
            }

            base.PeerTCPMessage(server, session, reader);
        }
    }
}
