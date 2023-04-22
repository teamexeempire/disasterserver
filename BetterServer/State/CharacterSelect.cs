using BetterServer.Data;
using BetterServer.Maps;
using BetterServer.Session;
using ExeNet;
using System.Net;

namespace BetterServer.State
{
    internal class CharacterSelect : State
    {
        private Random _rand = new();
        private int _timeout = 0;

        private Peer _exe;
        private Map _map;
        private Dictionary<ushort, int> _lastPackets = new();

        public CharacterSelect(Map map) => _map = map;

        public override Session.State AsState()
        {
            return Session.State.CHARACTERSELECT;
        }

        public override void PeerJoined(Server server, TcpSession session, Peer peer)
        {
        }

        public override void PeerLeft(Server server, TcpSession session, Peer peer)
        {
            lock (server.Peers)
            {
                if (server.Peers.Count <= 1 || _exe == peer)
                {
                    server.SetState<Lobby>();
                    return;
                }

                var cnt = 0;
                foreach (var pr in server.Peers.Values)
                    if (pr.Player.Character != Character.None)
                        cnt++;

                if (cnt >= server.Peers.Count)
                    server.SetState(new Game(_map, _exe.ID));
            }

            lock (_lastPackets)
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
                            if (server.Peers[session.ID].Player.Character != Character.None)
                                break;

                            foreach (var peer in server.Peers.Values)
                            {
                                if (peer.Player.Character == (Character)id)
                                    canUse = false;

                                if (peer.Player.Character != Character.None)
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

                                Terminal.LogDiscord($"{peer.Nickname} chooses {(Character)id}");

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
                if (server.Peers.Count <= 1)
                    server.SetState<Lobby>();

                var ind = 0;

                foreach (var peer in server.Peers.Values)
                {
                    if (peer.Pending)
                    {
                        server.DisconnectWithReason(server.GetSession(peer.ID), "PacketType.CLIENT_REQUESTED_INFO missing.");
                        continue;
                    }

                    lock (_lastPackets)
                        _lastPackets.Add(peer.ID, 0);

                    /* Reset player information */
                    peer.Player = new();

                    ind++;
                }

                // Pick random map
                _exe = ChooseExe(server) ?? server.Peers[0]; // never will be null
                _exe.Player.Character = Character.Exe; 
                _exe.ExeChance = 0; // reset chance

                foreach (var peer in server.Peers.Values)
                {
                    if (peer.Player.Character != Character.Exe)
                        peer.ExeChance += _rand.Next(4, 10);
                    else
                        peer.ExeChance += _rand.Next(0, 2);
                }

                Terminal.LogDiscord($"Map is {_map}");

                var packet = new TcpPacket(PacketType.SERVER_LOBBY_EXE);
                packet.Write((ushort)_exe.ID);
                packet.Write((ushort)Array.IndexOf(MapVote.Maps, _map?.GetType()));
                server.TCPMulticast(packet);
            }
        }

        private Peer? ChooseExe(Server server)
        {
            Dictionary<ushort, double> chances = new();
            double accWeight = 0;

            lock (server.Peers)
            {
                foreach (var peer in server.Peers.Values)
                {
                    accWeight += peer.ExeChance;
                    chances.Add(peer.ID, accWeight);
                }

                double r = _rand.NextDouble() * accWeight;
                foreach (var chance in chances)
                {
                    if (chance.Value >= r)
                        return server.Peers.Values.FirstOrDefault(e => e.ID == chance.Key);
                }
            }

            // Will never happen
            return null;
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
                        if (peer.Player.Character != Character.None)
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