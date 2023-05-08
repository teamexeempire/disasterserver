using BetterServer.Data;
using BetterServer.Maps;
using BetterServer.Session;
using ExeNet;
using System.Net;
using System.Net.Sockets;
using System.Xml.Linq;

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
            lock(server.Peers)
            {
                if(server.Peers.Count <= 1)
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
        }

        public override void PeerTCPMessage(Server server, TcpSession session, BinaryReader reader)
        {
            var passtrough = reader.ReadBoolean();
            var type = reader.ReadByte();

            if (passtrough)
                server.Passtrough(reader, session);

            lock(_lastPackets)
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

                                Terminal.Log($"Sending {player.Value.Nickname}'s data.");

                                var pk = new TcpPacket(PacketType.SERVER_LOBBY_PLAYER);
                                pk.Write(player.Value.ID);
                                pk.Write(player.Value.Player.IsReady);
                                pk.Write(player.Value.Nickname);

                                server.TCPSend(session, pk);
                            }
                        }
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
                            if (server.Peers.ContainsKey(session.ID))
                            {
                                server.Peers[session.ID].Pending = false;
                                server.Peers[session.ID].Nickname = name;

                                Terminal.LogDiscord($"{name} (ID {server.Peers[session.ID].ID}) joined.");
                                packet.Write(server.Peers[session.ID].ID);
                                packet.Write(name);
                            }
                        }
                        server.TCPMulticast(packet, session.ID);
                    }
                    break;

                /* Chat message */
                case PacketType.CLIENT_CHAT_MESSAGE:
                    {
                        var id = reader.ReadUInt16();
                        var msg = reader.ReadStringNull();

                        if (msg == "mermerzzzhruk")
                        {
                            server.SetState<CharacterSelect>(new CharacterSelect(new FartZone()));
                            break;
                        }


                        lock (server.Peers)
                        {
                            foreach (var peer in server.Peers.Values)
                            {
                                if (peer.ID != id)
                                    continue;

                                Terminal.LogDiscord($"[{peer.Nickname}]: {msg}");
                            }
                        }
                        break;
                    }
                
                /* New ready state (key Z) */
                case PacketType.CLIENT_LOBBY_READY_STATE:
                    {
                        var ready = reader.ReadBoolean();

                        lock(server.Peers)
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
            }
        }

        /* Lobby has no UDP messages */
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

                    peer.Player = new();

                    if (peer.ExeChance >= 99)
                        peer.ExeChance = 99;

                    var packet = new TcpPacket(PacketType.SERVER_LOBBY_EXE_CHANCE, (byte)peer.ExeChance);
                    server.TCPSend(server.GetSession(peer.ID), packet);
                }
            }

            var pk = new TcpPacket(PacketType.SERVER_GAME_BACK_TO_LOBBY);
            server.TCPMulticast(pk);
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
                else if(_isCounting)
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
    }
}
