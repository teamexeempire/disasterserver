using BetterServer.Entities;
using BetterServer.Session;
using ExeNet;

namespace BetterServer.Maps
{
    public class LimpCity : Map
    {
        public override void Init(Server server)
        {
            Spawn(server, new LCEye() { ID = 0 });
            Spawn(server, new LCEye() { ID = 1 });
            Spawn<LCChainController>(server);
            SetTimer(server, 155);
            
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

            switch ((PacketType)type)
            {
                case PacketType.CLIENT_LCEYE_REQUEST_ACTIVATE:
                    {
                        var value = reader.ReadBoolean();
                        var nid = reader.ReadByte();
                        var target = reader.ReadByte();

                        lock (Entities)
                        {
                            var list = FindOfType<LCEye>();
                            if (list == null)
                                break;

                            if (nid >= list.Length)
                                break;

                            var eye = list[nid];
                            if (value)
                            {
                                if (eye.Used)
                                    break;

                                if (eye.Charge < 20)
                                    break;

                                lock (server.Peers)
                                    eye.UseID = server.Peers[session.ID].ID;

                                eye.Target = target;
                                eye.Used = true;
                                eye.SendState(server);
                            }
                            else
                            {
                                eye.Used = false;
                                eye.UseID = 0;
                                eye.SendState(server);
                            }
                        }
                        break;
                    }
            }

            base.PeerTCPMessage(server, session, reader);
        }
    }
}
