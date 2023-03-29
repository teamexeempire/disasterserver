using BetterServer.Data;
using BetterServer.Entities;
using BetterServer.Maps;
using BetterServer.Session;
using ExeNet;
using System.Net;
using System.Net.Sockets;

namespace BetterServer.State
{
    public class Game : State
    {
        public Dictionary<ushort, IPEndPoint> IPEndPoints = new();

        private bool _waiting = true;
        private Map _map;
        private readonly ushort _exeId;
        private int _endTimer = -1;
        private int _demonCount = 0;
        private int _timeout = 1 * Ext.FRAMESPSEC;

        private Dictionary<ushort, int> _lastPackets = new();

        public Game(Map map, ushort exe)
        {
            _map = map;
            _map.Game = this;
            _exeId = exe;
        }

        public override Session.State AsState()
        {
            return Session.State.GAME;
        }

        public override void PeerJoined(Server server, TcpSession session, Peer peer)
        {
        }

        public override void PeerLeft(Server server, TcpSession session, Peer peer)
        {
            lock (server.Peers)
            {
                if (peer.Player.RevivalTimes >= 2)
                    _demonCount--;

                if (_exeId == peer.ID)
                {
                    EndGame(server, 1);
                    return;
                }

                if (server.Peers.Count <= 1)
                    server.SetState<Lobby>();
            }

            lock (IPEndPoints)
                IPEndPoints.Remove(peer.ID);

            lock (_lastPackets)
                _lastPackets.Remove(peer.ID);
        }

        public override void PeerTCPMessage(Server server, TcpSession session, BinaryReader reader)
        {
            // Actual handlin
            Logger.LogDebug("HandlePlayers()");
            HandlePlayers(server, session, reader);

            Logger.LogDebug("HandleMap()");
            reader.BaseStream.Seek(0, SeekOrigin.Begin);
            _map.PeerTCPMessage(server, session, reader);
        }


        public override void PeerUDPMessage(Server server, IPEndPoint endpoint, BinaryReader reader)
        {
            try
            {
                if (reader.BaseStream.Length <= 0)
                {
                    Logger.LogDebug($"Length is 0 from {endpoint}");
                    return;
                }

                var pid = reader.ReadUInt16();

                // Waiting for packets from all players
                if (_waiting)
                {
                    lock (IPEndPoints)
                    {
                        if (!IPEndPoints.ContainsKey(pid))
                        {
                            Logger.LogDebug($"Received from {endpoint}");
                            IPEndPoints.Add(pid, endpoint);
                        }

                        lock (server.Peers)
                        {
                            if (IPEndPoints.Count >= server.Peers.Count)
                            {
                                _map.Init(server);

                                Logger.LogDiscord("Got packets from all players.");
                                _waiting = false;
                            }
                        }
                    }
                    return;
                }

                var type = (PacketType)reader.ReadByte();
                var pack = new UdpPacket(type);
                while (reader.BaseStream.Position < reader.BaseStream.Length)
                    pack.Write(reader.ReadByte());

                server.UDPMulticast(ref IPEndPoints, pack, endpoint);
                reader.BaseStream.Position = 3;

                _lastPackets[pid] = 0;
                switch (type)
                {
                    case PacketType.CLIENT_PING:
                        {
                            var pk = new UdpPacket(PacketType.SERVER_PONG);
                            server.UDPSend(endpoint, pk);
                            return;
                        }
                }
            }
            catch (Exception e)
            {
                Logger.LogDebug($"Exception from {endpoint} UDP: {e}");
            }
        }

        public override void UDPSocketError(IPEndPoint endpoint, SocketError error)
        {            
            lock(IPEndPoints)
            {
                var item = IPEndPoints.FirstOrDefault(kvp => kvp.Value == endpoint);
                IPEndPoints.Remove(item.Key);
            }

            base.UDPSocketError(endpoint, error);
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
            }

            var packet = new TcpPacket(PacketType.SERVER_LOBBY_GAME_START);
            server.TCPMulticast(packet);

            Logger.LogDiscord("Waiting for players...");
        }

        public override void Tick(Server server)
        {
            if (_endTimer > 0)
            {
                _endTimer--;
                return;
            }

            if (_endTimer == 0)
            {
                server.SetState<Lobby>();
                return;
            }

            if (_waiting)
                return;

            DoTimeout(server);
            UpdateDeathTimers(server);
            _map.Tick(server);

            /* Check time */
            if (_map.Timer <= 0)
                EndGame(server, 2);
        }

        private void DoTimeout(Server server)
        {
            if (_timeout-- > 0)
                return;

            lock (server.Peers)
            {
                foreach (var peer in server.Peers.Values)
                {
                    if (peer.Player.HasEscaped || !peer.Player.IsAlive)
                    {
                        _lastPackets[peer.ID] = 0;
                        continue;
                    }

                    if (_lastPackets[peer.ID] >= 4 * Ext.FRAMESPSEC)
                        server.DisconnectWithReason(server.GetSession(peer.ID), "AFK or Timeout");

                    _lastPackets[peer.ID] += Ext.FRAMESPSEC;
                }
            }

            _timeout = 1 * Ext.FRAMESPSEC;
        }

        private void HandlePlayers(Server server, TcpSession session, BinaryReader reader)
        {
            var passtrough = reader.ReadBoolean();
            var type = reader.ReadByte();

            if (passtrough)
                server.Passtrough(reader, session);

            switch ((PacketType)type)
            {
                /* If player dies */
                case PacketType.CLIENT_PLAYER_DEATH_STATE:
                    {
                        if (_endTimer >= 0)
                            break;

                        lock (server.Peers)
                        {
                            if (!server.Peers.ContainsKey(session.ID))
                                break;

                            if (server.Peers[session.ID].Player.RevivalTimes >= 2)
                                break;

                            var peer = server.Peers[session.ID];

                            // Set flags
                            peer.Player.IsAlive = !reader.ReadBoolean();
                            peer.Player.RevivalTimes = reader.ReadByte();

                            if (!peer.Player.IsAlive)
                            {
                                Logger.LogDiscord($"{peer.Nickname} died.");

                                if (peer.Player.DiedBefore || _map.Timer <= Ext.FRAMESPSEC * Ext.FRAMESPSEC * 2)
                                {
                                    var pkt = new TcpPacket(PacketType.SERVER_GAME_DEATHTIMER_END);

                                    if (_demonCount >= (int)((server.Peers.Count - 1) / 2.0))
                                        pkt.Write(0);
                                    else
                                    {
                                        peer.Player.RevivalTimes = 2;
                                        _demonCount++;

                                        Logger.LogDiscord($"{peer.Nickname} was demonized!");

                                        pkt.Write(1);
                                    }

                                    server.TCPSend(session, pkt);
                                    peer.Player.DeadTimer = -1;
                                }

                                if (peer.Player.RevivalTimes == 0)
                                    peer.Player.DeadTimer = 30 * Ext.FRAMESPSEC;

                                peer.Player.DiedBefore = true;
                            }
                            else
                                peer.Player.DeadTimer = -1;

                            CheckEscapedAndAlive(server, peer);
                        }
                        break;
                    }

                case PacketType.CLIENT_PLAYER_ESCAPED:
                    {
                        if (_endTimer >= 0)
                            break;

                        lock (server.Peers)
                        {
                            if (!server.Peers.ContainsKey(session.ID))
                                break;

                            var peer = server.Peers[session.ID];
                            peer.Player.HasEscaped = true;

                            var pk = new TcpPacket(PacketType.SERVER_GAME_PLAYER_ESCAPED);
                            pk.Write(peer.ID);

                            server.TCPMulticast(pk);
                            CheckEscapedAndAlive(server, peer);

                            Logger.LogDiscord($"{peer.Nickname} has escaped!");
                        }
                        break;
                    }
            }
        }

        private void UpdateDeathTimers(Server server)
        {
            lock (server.Peers)
            {
                foreach (var peer in server.Peers.Values)
                {
                    if (peer.Player.IsAlive || peer.Player.HasEscaped)
                    {
                        peer.Player.DeadTimer = -1;
                        continue;
                    }

                    if (peer.Player.DeadTimer == -1)
                        continue;

                    if (peer.Player.DeadTimer % Ext.FRAMESPSEC == 0)
                    {
                        var pk = new TcpPacket(PacketType.SERVER_GAME_DEATHTIMER_TICK);
                        pk.Write((ushort)peer.ID);
                        pk.Write((byte)(peer.Player.DeadTimer / Ext.FRAMESPSEC));
                        server.TCPMulticast(pk);
                    }

                    peer.Player.DeadTimer--;
                    if (peer.Player.DeadTimer <= 0 || _map.Timer <= Ext.FRAMESPSEC * Ext.FRAMESPSEC * 2)
                    {
                        var pkt = new TcpPacket(PacketType.SERVER_GAME_DEATHTIMER_END);

                        if (_demonCount >= (int)((server.Peers.Count - 1) / 2.0))
                            pkt.Write(0);
                        else
                        {
                            _demonCount++;
                            peer.Player.RevivalTimes = 2;

                            Logger.LogDiscord($"{peer.Nickname} was demonized!");
                            pkt.Write(1);
                        }

                        server.TCPSend(server.GetSession(peer.ID), pkt);
                        peer.Player.DeadTimer = -1;
                    }
                }
            }
        }

        private void CheckEscapedAndAlive(Server server, Peer peer)
        {
            lock (server.Peers)
            {
                if (_endTimer >= 0)
                    return;

                if (server.Peers.Count <= 0)
                {
                    server.SetState<Lobby>();
                    return;
                }

                if (!server.Peers.ContainsKey(_exeId))
                {
                    EndGame(server, 1);
                    return;
                }

                var alive = 0;
                var escaped = 0;

                foreach (var player in server.Peers)
                {
                    if (player.Key == _exeId)
                        continue;

                    if (player.Value.Player.IsAlive)
                        alive++;

                    if (player.Value.Player.HasEscaped)
                        escaped++;
                }

                // Exe wins
                if (alive == 0)
                {
                    EndGame(server, 0);
                    return;
                }

                if ((server.Peers.Count - alive) + escaped >= server.Peers.Count)
                {
                    if (escaped == 0)
                        EndGame(server, 0);
                    else
                        EndGame(server, 1);
                }
            }
        }

        private void EndGame(Server server, int type)
        {
            if (_endTimer >= 0)
                return;

            if (type == 0) // exe win
            {
                _endTimer = 5 * Ext.FRAMESPSEC;

                Logger.LogDiscord($"Exe wins!");
                var pk = new TcpPacket(PacketType.SERVER_GAME_EXE_WINS);
                server.TCPMulticast(pk);
            }
            else if (type == 1) // survivors win
            {
                _endTimer = 5 * Ext.FRAMESPSEC;

                Logger.LogDiscord($"Survivors win!");
                var pk = new TcpPacket(PacketType.SERVER_GAME_SURVIVOR_WIN);
                server.TCPMulticast(pk);
            }
            else if (type == 2) // time over
            {
                _endTimer = 5 * Ext.FRAMESPSEC;

                Logger.LogDiscord($"Time over!");
                var pk = new TcpPacket(PacketType.SERVER_GAME_TIME_OVER);
                server.TCPMulticast(pk);

                // Send last time
                var packet = new TcpPacket(PacketType.SERVER_GAME_TIME_SYNC);
                packet.Write((ushort)0);
                server.TCPMulticast(packet);
            }
        }
    }
}
