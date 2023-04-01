using BetterServer.Data;
using BetterServer.Maps;
using BetterServer.Session;
using ExeNet;
using System.Net;

namespace BetterServer.State
{
    internal class CharacterSelect : State
    {
        private Random _rand = new Random();
        private int _timeout = 0;
        
        private Peer _exe;
        private Map _map;
        private Dictionary<ushort, int> _lastPackets = new();

        /* List of possible maps */
        private readonly Type[] Maps = new Type[]
        {
            typeof(HideAndSeek2),
            typeof(RavineMist),
            typeof(DotDotDot),
            typeof(DesertTown),
            typeof(YouCantRun),
            typeof(LimpCity),
            typeof(NotPefect),
            typeof(KindAndFair),
            typeof(Act9)
        };

        public override Session.State AsState()
        {
            return Session.State.CHARACTERSELECT;
        }

        public override void PeerJoined(Server server, TcpSession session, Peer peer)
        {
        }

        public override void PeerLeft(Server server, TcpSession session, Peer peer)
        {
            lock(server.Peers)
            {
                if (server.Peers.Count <= 1 || _exe == peer)
                {
                    server.SetState<Lobby>();
                    return;
                }

                var cnt = 0;
                foreach (var pr in server.Peers.Values)
                    if (pr.Player.Character != Character.NONE)
                        cnt++;

                if(cnt >= server.Peers.Count)
                    server.SetState(new Game(_map, _exe.ID));
            }

            lock(_lastPackets)
                _lastPackets.Remove(peer.ID);
        }

        public override void PeerTCPMessage(Server server, TcpSession session, BinaryReader reader)
        {
            lock (_lastPackets)
                _lastPackets[session.ID] = 0;

            var passtrough = reader.ReadBoolean();
            var type = reader.ReadByte();

            if (passtrough)
                server.Passtrough(reader, session);

            switch ((PacketType)type)
            {
                case PacketType.CLIENT_REQUEST_CHARACTER:
                    {
                        var id = reader.ReadByte();
                        var canUse = true;
                        var cnt = 0;

                        lock (server.Peers)
                        {
                            if (server.Peers[session.ID].Player.Character != Character.NONE)
                                break;

                            Logger.Log($"{id}");
                            foreach (var peer in server.Peers.Values)
                            {
                                if (peer.Player.Character == (Character)id)
                                    canUse = false;

                                if (peer.Player.Character != Character.NONE)
                                    cnt++;
                            }

                            if (canUse)
                            {
                                if (!server.Peers.ContainsKey(session.ID))
                                    break;

                                var peer = server.Peers[session.ID];
                                peer.Player.Character = (Character)id;

                                var packet = new TcpPacket(PacketType.SERVER_LOBBY_CHARACTER_RESPONSE, id, true);
                                server.TCPSend(session, packet);

                                packet = new TcpPacket(PacketType.SERVER_LOBBY_CHARACTER_CHANGE);
                                packet.Write(peer.ID);
                                packet.Write(id);
                                server.TCPMulticast(packet, session.ID);

                                Logger.LogDiscord($"{peer.Nickname} chooses {(Character)id}");

                                if (++cnt >= server.Peers.Count)
                                    server.SetState(new Game(_map, _exe.ID));
                            }
                            else
                            {
                                var packet = new TcpPacket(PacketType.SERVER_LOBBY_CHARACTER_RESPONSE, id, false);
                                server.TCPSend(session, packet);
                            }
                        }

                        break;
                    }
            }
        }
        
        public override void PeerUDPMessage(Server server, IPEndPoint IPEndPoint, BinaryReader reader)
        {
        }

        public override void Init(Server server)
        {
            lock (server.Peers)
            {
                foreach (var peer in server.Peers.Values)
                {
                    lock (_lastPackets)
                        _lastPackets.Add(peer.ID, 0);
                }

                if (server.Peers.Count <= 1)
                    server.SetState<Lobby>();

                var exe = _rand.Next(server.Peers.Count);
                var ind = 0;

                foreach(var peer in server.Peers.Values)
                {
                    if (peer.Pending)
                    {
                        server.DisconnectWithReason(server.GetSession(peer.ID), "PacketType.CLIENT_REQUESTED_INFO missing.");
                        continue;
                    }

                    /* Reset player information */
                    peer.Player = new();

                    if (exe == ind)
                        _exe = peer;

                    ind++;
                }

                // Pick random map
                _map = Ext.CreateOfType<Map>(Maps[_rand.Next(Maps.Length)]);
                //_map = Ext.CreateOfType<NotPefect>();
                _exe.Player.Character = Character.EXE;

                Logger.LogDiscord($"Map is {_map}");

                var packet = new TcpPacket(PacketType.SERVER_LOBBY_EXE);
                packet.Write((ushort)_exe.ID);
                packet.Write((ushort)Array.IndexOf(Maps, _map?.GetType()));
                server.TCPMulticast(packet);
            }
        }

        public override void Tick(Server server)
        {
            DoTimeout(server);
        }

        private void DoTimeout(Server server)
        {
            if (_timeout-- > 0)
                return;

            lock (server.Peers)
            {
                foreach (var peer in server.Peers.Values)
                {
                    lock (_lastPackets)
                    {
                        if (peer.Player.Character != Character.NONE)
                        {
                            _lastPackets[peer.ID] = 0;
                            continue;
                        }

                        if (_lastPackets[peer.ID] >= 20 * Ext.FRAMESPSEC)
                            server.DisconnectWithReason(server.GetSession(peer.ID), "AFK or Timeout");

                        _lastPackets[peer.ID] += Ext.FRAMESPSEC;
                    }
                }
            }

            _timeout = 1 * Ext.FRAMESPSEC;
        }
    }
}