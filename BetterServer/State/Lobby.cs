using BetterServer.Data;
using BetterServer.Maps;
using BetterServer.Session;
using ExeNet;
using System.Net;

namespace BetterServer.State
{
    public class Lobby : State
    {
        private bool _isCounting = false;
        private int _countdown = 5 * Ext.FRAMESPSEC;
        private int _timeout = 1 * Ext.FRAMESPSEC;
        private Mutex _lock = new();
        private Random _rand = new();

        private Dictionary<ushort, int> _lastPackets = new();

        private ushort _voteKickTarget = ushort.MaxValue;
        private int _voteKickTimer = 0;
        private List<ushort> _voteKickVotes = new();
        private bool _voteKick = false;
        private int _voteKickCount = 0;

        private bool _practice = false;
        private int _practiceCount = 0;
        private List<ushort> _practiceVotes = new();

        public override Session.State AsState()
        {
            return Session.State.LOBBY;
        }

        public override void PeerJoined(Server server, TcpSession session, Peer peer)
        {
            if (_isCounting)
            {
                _isCounting = false;

                // For safety
                _lock.WaitOne(1000);
                {
                    _countdown = 5 * Ext.FRAMESPSEC;
                }
                _lock.ReleaseMutex();

                /* Send update since new player joined */
                MulticastState(server);
            }

            lock (_lastPackets)
                _lastPackets.Add(peer.ID, 0);

            peer.ExeChance = _rand.Next(2, 5);

            var packet = new TcpPacket(PacketType.SERVER_LOBBY_EXE_CHANCE, (byte)peer.ExeChance);
            server.TCPSend(session, packet);

        }

        public override void PeerLeft(Server server, TcpSession session, Peer peer)
        {
            lock (server.Peers)
            {
                if (server.Peers.Count <= 1)
                {
                    _isCounting = false;

                    // For safety
                    _lock.WaitOne(1000);
                    {
                        _countdown = 5 * Ext.FRAMESPSEC;
                    }
                    _lock.ReleaseMutex();

                    /* Send update since new player joined */
                    MulticastState(server);
                }

                lock (_lastPackets)
                    _lastPackets.Remove(peer.ID);

                Terminal.LogDiscord($"{peer.Nickname} (ID {peer.ID}) left.");
            }

            lock (_voteKickVotes)
            {
                if (!_voteKick)
                    return;

                if (_voteKickVotes.Contains(session.ID))
                    _voteKickVotes.Remove(session.ID);

                if(_voteKickTarget == session.ID)
                {
                    Terminal.LogDiscord($"Vote kick failed for {peer.Nickname} (PID {peer.ID}) because player left.");

                    SendMessage(server, $"\\kick vote failed~ (player left)");
                    _voteKickVotes.Clear();
                    _voteKickTimer = 0;
                    _voteKickTarget = ushort.MaxValue;
                    _voteKick = false;
                    return;
                }

                if (_voteKickVotes.Count >= _voteKickCount)
                    CheckVoteKick(server, false);
            }

            lock (_practiceVotes)
            {
                if (_practiceVotes.Contains(session.ID))
                    _practiceVotes.Remove(session.ID);
            }
        }

        public override void PeerTCPMessage(Server server, TcpSession session, BinaryReader reader)
        {
            var passtrough = reader.ReadBoolean();
            var type = reader.ReadByte();

            if (passtrough)
                server.Passtrough(reader, session);

            lock (_lastPackets)
                _lastPackets[session.ID] = 0;

            switch ((PacketType)type)
            {
                /* Player requests player list */
                case PacketType.CLIENT_LOBBY_PLAYERS_REQUEST:
                    {
                        lock (server.Peers)
                        {
                            foreach (var player in server.Peers)
                            {
                                if (player.Value.Pending)
                                    continue;

                                if (player.Key == session.ID)
                                    continue;

                                Terminal.LogDebug($"Sending {player.Value.Nickname}'s data to PID {session.ID}");

                                var pk = new TcpPacket(PacketType.SERVER_LOBBY_PLAYER);
                                pk.Write(player.Value.ID);
                                pk.Write(player.Value.Player.IsReady);
                                pk.Write(player.Value.Nickname[..Math.Min(player.Value.Nickname.Length, 15)]);
                                pk.Write(player.Value.Icon);
                                pk.Write(player.Value.Pet);

                                server.TCPSend(session, pk);
                            }
                        }

                        server.TCPSend(session, new TcpPacket(PacketType.SERVER_LOBBY_CORRECT));
                        SendMessage(server, session, "|type .help for command list~");
                        break;
                    }

                /* Info requested by server */
                case PacketType.CLIENT_REQUESTED_INFO:
                    {
                        var ver = reader.ReadUInt16();
                        if (ver != Program.BUILD_VER)
                        {
                            server.DisconnectWithReason(session, $"Wrong game version ({Program.BUILD_VER} required, but got {ver})");
                            break;
                        }

                        var packet = new TcpPacket(PacketType.SERVER_PLAYER_INFO);
                        lock (server.Peers)
                        {
                            var name = reader.ReadStringNull();
                            var icon = reader.ReadByte();
                            var pet = reader.ReadSByte();

                            if (server.Peers.ContainsKey(session.ID))
                            {
                                server.Peers[session.ID].Pending = false;
                                server.Peers[session.ID].Nickname = Ext.ValidateNick(name);
                                server.Peers[session.ID].Icon = icon;
                                server.Peers[session.ID].Pet = pet;

                                Terminal.LogDiscord($"{name} (ID {server.Peers[session.ID].ID}) joined.");
                                packet.Write(server.Peers[session.ID].ID);
                                packet.Write(name);
                                packet.Write(icon);
                                packet.Write(pet);

                                Program.Window.AddPlayer(server.Peers[session.ID]);
                            }
                        }
                        server.TCPMulticast(packet, session.ID);

                        Program.Stat?.MulticastInformation();
                    }
                    break;

                /* Chat message */
                case PacketType.CLIENT_CHAT_MESSAGE:
                    {
                        var id = reader.ReadUInt16();
                        var msg = reader.ReadStringNull();

                        lock (server.Peers)
                        {
                            switch (msg)
                            {
                                case ".y":
                                case ".yes":
                                    lock (_voteKickVotes)
                                    {
                                        if (_voteKickTimer <= 0)
                                            break;

                                        if (!_voteKickVotes.Contains(session.ID))
                                            _voteKickVotes.Add(session.ID);
                                        else
                                            break;

                                        lock (server.Peers)
                                        {
                                            SendMessage(server, $"{server.Peers[session.ID].Nickname} voted @yes~");
                                            Terminal.LogDiscord($"{server.Peers[session.ID].Nickname} voted yes");
                                        }

                                        if (_voteKickVotes.Count >= _voteKickCount)
                                            CheckVoteKick(server, false);
                                    }

                                    break;

                                case ".n":
                                case ".no":
                                    lock (_voteKickVotes)
                                    {
                                        if (_voteKickTimer <= 0)
                                            break;

                                        if (_voteKickVotes.Contains(session.ID))
                                            _voteKickVotes.Remove(session.ID);
                                        
                                        lock (server.Peers)
                                        {
                                            SendMessage(server, $"{server.Peers[session.ID].Nickname} voted \\no~");
                                            Terminal.LogDiscord($"{server.Peers[session.ID].Nickname} voted no");
                                        }

                                        if (_voteKickVotes.Count >= _voteKickCount)
                                            CheckVoteKick(server, false);
                                    }
                                    break;

                                case ".help":
                                case ".h":
                                    SendMessage(server, session, $"~----------------------");
                                    SendMessage(server, session, "|list of commands:~");
                                    SendMessage(server, session, "@.practice~ (.p) - practice mode vote");
                                    SendMessage(server, session, "@.mute~ (.m) - toggle chat messages");
                                    SendMessage(server, session, "@.votekick~ (.vk) - kick vote a player");
                                    SendMessage(server, session, $"~----------------------");
                                    break;

                                case ".practice":
                                case ".p":
                                    if (_practice)
                                    {
                                        lock(_practiceVotes)
                                        {
                                            if(_practiceVotes.Contains(session.ID))
                                                break;

                                            lock (server.Peers)
                                            {
                                                SendMessage(server, $"{server.Peers[session.ID].Nickname} wants to `practice~");
                                                Terminal.LogDiscord($"{server.Peers[session.ID].Nickname} wants to practice");

                                                _practiceVotes.Add(session.ID);

                                                if (_practiceVotes.Count >= _practiceCount)
                                                    server.SetState(new CharacterSelect(new FartZone()));
                                            }
                                        }
                                        break;
                                    }

                                    lock (_practiceVotes)
                                    {
                                        lock (server.Peers)
                                        {
                                            SendMessage(server, $"{server.Peers[session.ID].Nickname} wants to `practice~");
                                            Terminal.LogDiscord($"{server.Peers[session.ID].Nickname} wants to practice");

                                            _practiceVotes.Add(session.ID);
                                        }
                                    }

                                    lock(server.Peers)
                                        _practiceCount = server.Peers.Count;

                                    Terminal.LogDiscord($"{server.Peers[session.ID].Nickname} started practice vote");
                                    SendMessage(server, $"~----------------------");
                                    SendMessage(server, $"\\`practice~ vote started by /{server.Peers[id].Nickname}~");
                                    SendMessage(server, $"type `.p~ for practice room");
                                    SendMessage(server, $"~----------------------");

                                    _practice = true;
                                    break;

                                default:
                                    foreach (var peer in server.Peers.Values)
                                    {
                                        if (peer.ID != id)
                                            continue;

                                        Terminal.LogDiscord($"[{peer.Nickname}]: {msg}");
                                    }
                                    break;
                            }
                        }
                        break;
                    }

                /* New ready state (key Z) */
                case PacketType.CLIENT_LOBBY_READY_STATE:
                    {
                        var ready = reader.ReadBoolean();

                        lock (server.Peers)
                        {
                            if (!server.Peers.ContainsKey(session.ID))
                                break;

                            var peer = server.Peers[session.ID];
                            peer.Player.IsReady = ready;

                            var pk = new TcpPacket(PacketType.SERVER_LOBBY_READY_STATE);
                            pk.Write(peer.ID);
                            pk.Write(ready);
                            server.TCPMulticast(pk, session.ID);

                            CheckReadyPeers(server);
                        }
                        break;
                    }

                case PacketType.CLIENT_LOBBY_VOTEKICK:
                    {
                        var id = reader.ReadUInt16();

                        lock (server.Peers)
                        {
                            lock (_voteKickVotes)
                            {
                                if (id == _voteKickTarget)
                                {
                                    if (!_voteKickVotes.Contains(session.ID))
                                    {
                                        _voteKickVotes.Add(session.ID);

                                        SendMessage(server, $"{server.Peers[session.ID].Nickname} voted @yes~");

                                        if (_voteKickVotes.Count >= _voteKickCount)
                                            CheckVoteKick(server, false);
                                    }
                                }
                                else if (_voteKick)
                                    SendMessage(server, session, "\\kick vote is already in process!");
                                else
                                {
                                    VoteKickStart(server, session.ID, id);
                                    SendMessage(server, $"{server.Peers[session.ID].Nickname} voted @yes~");
                                }
                            }
                        }

                        break;
                    }
            }
        }

        /* Lobby has no UDP messages */
        public override void PeerUDPMessage(Server server, IPEndPoint IPEndPoint, ref byte[] data)
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

                    peer.Player = new();

                    if (peer.ExeChance >= 99)
                        peer.ExeChance = 99;

                    var packet = new TcpPacket(PacketType.SERVER_LOBBY_EXE_CHANCE, (byte)peer.ExeChance);
                    server.TCPSend(server.GetSession(peer.ID), packet);
                }
            }

            var pk = new TcpPacket(PacketType.SERVER_GAME_BACK_TO_LOBBY);
            server.TCPMulticast(pk);

            Program.Stat?.MulticastInformation();
        }

        public override void Tick(Server server)
        {
            if (_isCounting)
            {
                // For safety
                _lock.WaitOne(1000);
                {
                    _countdown--;

                    if (_countdown <= 0)
                    {
                        CheckVoteKick(server, true);
                        server.SetState<MapVote>();
                    }
                    else
                    {
                        if (_countdown % Ext.FRAMESPSEC == 0)
                            MulticastState(server);
                    }


                }
                _lock.ReleaseMutex();
            }

            DoVoteKick(server);
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
                        if (peer.Player.IsReady)
                        {
                            _lastPackets[peer.ID] = 0;
                            continue;
                        }

                        if (_lastPackets[peer.ID] >= 30 * Ext.FRAMESPSEC)
                            server.DisconnectWithReason(server.GetSession(peer.ID), "AFK or Timeout");

                        _lastPackets[peer.ID] += Ext.FRAMESPSEC;
                    }
                }
            }

            _timeout = 1 * Ext.FRAMESPSEC;
        }

        private void CheckReadyPeers(Server server)
        {
            lock (server.Peers)
            {
                var totalReady = 0;

                foreach (var pr in server.Peers.Values)
                {
                    if (pr.Player.IsReady)
                        totalReady++;
                }

                if (totalReady >= server.Peers.Count && totalReady > 1)
                {
                    _isCounting = true;

                    MulticastState(server);
                }
                else if (_isCounting)
                {
                    _isCounting = false;

                    // For safety
                    _lock.WaitOne(1000);
                    {
                        _countdown = 5 * Ext.FRAMESPSEC;
                    }
                    _lock.ReleaseMutex();

                    MulticastState(server);
                }
            }
        }

        private void MulticastState(Server server)
        {
            var packet = new TcpPacket
            (
                PacketType.SERVER_LOBBY_COUNTDOWN,
                _isCounting,
                (byte)(_countdown / Ext.FRAMESPSEC) /* 300/60=5 */
            );

            server.TCPMulticast(packet);
        }

        private void DoVoteKick(Server server)
        {
            lock (_voteKickVotes)
            {
                if (_voteKickTimer > 0)
                {
                    _voteKickTimer--;
                    if (_voteKickTimer <= 0)
                    {
                        CheckVoteKick(server, true);
                    }
                }
            }
        }

        private void CheckVoteKick(Server server, bool ignore)
        {
            if (_voteKickTimer <= 0 && !ignore)
                return;

            if (!_voteKick)
                return;

            int totalFor = _voteKickVotes.Count;
            int totalAgainst = 0;

            lock (server.Peers)
            {
                if(!server.Peers.ContainsKey(_voteKickTarget))
                {
                    Terminal.LogDiscord($"Vote kick failed for PID {_voteKickTarget} because player left.");

                    SendMessage(server, $"\\kick vote failed~ (player left)");
                    _voteKickVotes.Clear();
                    _voteKickTimer = 0;
                    _voteKickTarget = ushort.MaxValue;
                    _voteKick = false;
                    return;
                }

                foreach(var peer in server.Peers)
                {
                    bool has = false;

                    foreach(var peer2 in _voteKickVotes)
                    {
                        if (peer.Key == peer2)
                        {
                            has = true;
                            break;
                        }
                    }

                    if(!has)
                        totalAgainst++;
                }
            }

            if(totalFor >= totalAgainst)
            {
                Terminal.LogDiscord($"Vote kick succeeded for {server.Peers[_voteKickTarget].Nickname} (PID {_voteKickTarget})");

                var session = server.GetSession(_voteKickTarget);
                KickList.Add((session.RemoteEndPoint! as IPEndPoint).Address.ToString()!);

                server.DisconnectWithReason(session, "Vote kick.");
                SendMessage(server, $"@kick vote success~ (@{totalFor}~ vs \\{totalAgainst}~)");
                _voteKickVotes.Clear();
                _voteKickTimer = 0;
                _voteKickTarget = ushort.MaxValue;
                _voteKick = false;
            }
            else
            {
                Terminal.LogDiscord($"Vote kick failed for {server.Peers[_voteKickTarget].Nickname} (PID {_voteKickTarget})");

                SendMessage(server, $"\\kick vote failed~ (@{totalFor}~ vs \\{totalAgainst}~)");
                _voteKickVotes.Clear();
                _voteKickTimer = 0;
                _voteKickTarget = ushort.MaxValue;
                _voteKick = false;
            }
        }

        private void VoteKickStart(Server server, ushort voter, ushort id)
        {
            _voteKickTarget = id;
            _voteKickVotes.Clear();
            _voteKickVotes.Add(voter);
            _voteKickTimer = Ext.FRAMESPSEC * 15;

            lock(server.Peers)
                _voteKickCount = server.Peers.Count - 1;
            
            _voteKick = true;

            SendMessage(server, $"~----------------------");
            SendMessage(server, $"\\kick vote started for /{server.Peers[id].Nickname}~");
            SendMessage(server, $"type @.y~ or \\.n~");
            SendMessage(server, $"~----------------------");

            Terminal.LogDiscord($"Vote kick started for {server.Peers[id].Nickname} (PID {id})");
        }

        private void SendMessage(Server server, string text)
        {
            var pack = new TcpPacket(PacketType.CLIENT_CHAT_MESSAGE, (ushort)0);
            pack.Write(text);
            server.TCPMulticast(pack);
        }

        private void SendMessage(Server server, ushort id, string text)
        {
            var pack = new TcpPacket(PacketType.CLIENT_CHAT_MESSAGE, (ushort)0);
            pack.Write(text);
            server.TCPSend(server.GetSession(id), pack);
        }

        private void SendMessage(Server server, TcpSession session, string text)
        {
            var pack = new TcpPacket(PacketType.CLIENT_CHAT_MESSAGE, (ushort)0);
            pack.Write(text);
            server.TCPSend(session, pack);
        }
    }
}
