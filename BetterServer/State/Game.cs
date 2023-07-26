using BetterServer.Data;
using BetterServer.Maps;
using BetterServer.Session;
using ExeNet;
using System.Linq;
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
        private bool _initMap = false;

        private Dictionary<ushort, int> _lastPackets = new();
        private Dictionary<ushort, int> _packetTimeouts = new();
        private Dictionary<ushort, RevivalData> _reviveTimer = new();

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
            lock (IPEndPoints)
                IPEndPoints.Remove(peer.ID);

            lock (_lastPackets)
                _lastPackets.Remove(peer.ID);

            lock (server.Peers)
            {
                if (peer.Player.RevivalTimes >= 2)
                    _demonCount--;

                CheckEscapedAndAlive(server);

                if (server.Peers.Count(e => !e.Value.Waiting) <= 1)
                    server.SetState<Lobby>();
            }

            _map.PeerLeft(server, session, peer);
        }

        public override void PeerTCPMessage(Server server, TcpSession session, BinaryReader reader)
        {
            // Actual handlin
            Terminal.LogDebug("HandlePlayers()");
            HandlePlayers(server, session, reader);

            Terminal.LogDebug("HandleMap()");
            _map.PeerTCPMessage(server, session, reader);
        }


        public override void PeerUDPMessage(Server server, IPEndPoint endpoint, ref byte[] data)
        {
            try
            {
                if (data.Length <= 0)
                {
                    Terminal.LogDebug($"Length is 0 from {endpoint}");
                    return;
                }

                FastBitReader reader = new();

                var pid = reader.ReadUShort(ref data);
                var type = (PacketType)reader.ReadByte(ref data);

                reader.Position = 3;
                _lastPackets[pid] = 0;
                switch (type)
                {
                    case PacketType.CLIENT_PING:
                        {
                            if (!IPEndPoints.Any(e => e.Value.ToString() == endpoint.ToString()) && !_waiting)
                            {
                                var session = server.GetSession(pid);

                                if(session == null)
                                    break;

                                Terminal.LogDebug($"ANTI SERG BOM BOM");
                                server.DisconnectWithReason(session, "invalid session");
                            }
                            else
                            {
                                Terminal.LogDebug($"Ping-pong with {endpoint} (PID {pid})");

                                var pk = new UdpPacket(PacketType.SERVER_PONG);
                                var ping = reader.ReadULong(ref data);
                                var calc = reader.ReadUShort(ref data);

                                pk.Write(ping);
                                server.UDPSend(endpoint, pk);

                                pk = new UdpPacket(PacketType.SERVER_GAME_PING);
                                pk.Write(pid);
                                pk.Write(calc);
                                server.UDPMulticast(ref IPEndPoints, pk, endpoint);
                            }
                            break;
                        }

                    case PacketType.CLIENT_PLAYER_DATA:
                        {
                            if (!_waiting)
                            {
                                var id = reader.ReadUShort(ref data);
                                var x = reader.ReadFloat(ref data);
                                var y = reader.ReadFloat(ref data);

                                lock (server.Peers)
                                {
                                    if (server.Peers.TryGetValue(id, out Peer? value))
                                    {
                                        if (value != null)
                                        {
                                            if (value.Player.Character == Character.Exe && value.Player.ExeCharacter == ExeCharacter.Original)
                                            {
                                                _ = reader.ReadByte(ref data); // state
                                                _ = reader.ReadUShort(ref data); // angle
                                                _ = reader.ReadByte(ref data); // index
                                                _ = reader.ReadChar(ref data); // scale 
                                                _ = reader.ReadByte(ref data); // effect time   
                                                _ = reader.ReadByte(ref data); // attacking
                                                _ = reader.ReadByte(ref data); // hurttime
                                                value.Player.Invisible = reader.ReadBoolean(ref data); // invisible
                                            }

                                            value.Player.X = x;
                                            value.Player.Y = y;
                                        }
                                    }
                                }

                                reader.Position = 3;

                                var pack = new UdpPacket(type);
                                while (reader.Position < data.Length)
                                    pack.Write(reader.ReadByte(ref data));

                                server.UDPMulticast(ref IPEndPoints, pack, endpoint);
                            }
                            break;
                        }
                }

                // Waiting for packets from all players
                if (_waiting)
                {
                    lock (IPEndPoints)
                    {
                        if (!IPEndPoints.ContainsKey(pid))
                        {
                            Terminal.LogDebug($"Received from {endpoint} (PID {pid})");
                            IPEndPoints.Add(pid, endpoint);

                            lock (_packetTimeouts)
                                _packetTimeouts[pid] = -1;
                        }

                        lock (server.Peers)
                        {
                            if (IPEndPoints.Count >= server.Peers.Count(e => !e.Value.Waiting))
                            {
                                lock (_map)
                                {
                                    if (!_initMap)
                                    {
                                        _map.Init(server);
                                        _initMap = true;
                                    }
                                }

                                Terminal.LogDiscord("Got packets from all players.");
                                _waiting = false;
                            }
                        }
                    }
                    return;
                }

                reader.Position = 0;
                _map.PeerUDPMessage(server, endpoint, data);
            }
            catch (Exception e)
            {
                Terminal.LogDebug($"PeerUDPMessage() failed for {endpoint}: {e}");
            }
        }

        public override void UDPSocketError(IPEndPoint endpoint, SocketError error)
        {
            Terminal.LogDebug($"Removing {endpoint}: {error}");
            lock (IPEndPoints)
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
                    if (peer.Waiting)
                        continue;

                    lock (_lastPackets)
                        _lastPackets.Add(peer.ID, 0);

                    lock (_packetTimeouts)
                        _packetTimeouts.Add(peer.ID, 18 * Ext.FRAMESPSEC);

                    lock (_reviveTimer)
                        _reviveTimer.Add(peer.ID, new());
                }
            }

            var packet = new TcpPacket(PacketType.SERVER_LOBBY_GAME_START);
            server.TCPMulticast(packet);

            Terminal.LogDiscord("Waiting for players...");
            Program.Stat?.MulticastInformation();
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
            {
                lock (_packetTimeouts)
                {
                    foreach (var pair in _packetTimeouts)
                    {
                        if (pair.Value == -1)
                            continue;

                        if (_packetTimeouts[pair.Key]-- <= 0)
                            server.DisconnectWithReason(server.GetSession(pair.Key), "UDP packets didnt arrive in time");
                    }
                }
                return;
            }

            lock (_reviveTimer)
            {
                foreach (var k in _reviveTimer)
                {
                    if (k.Value.Progress > 0)
                    {
                        k.Value.Progress -= 0.004;

                        if (k.Value.Progress <= 0)
                        {
                            k.Value.DeathNote.Clear();

                            server.TCPMulticast(new TcpPacket(PacketType.SERVER_REVIVAL_STATUS, false, k.Key));
                        }

                        server.UDPMulticast(ref IPEndPoints, new UdpPacket(PacketType.SERVER_REVIVAL_PROGRESS, k.Key, k.Value.Progress));
                    }
                    else k.Value.Progress = 0;
                }
            }

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
                lock (_lastPackets)
                {
                    foreach (var peer in server.Peers.Values)
                    {
                        if (peer.Waiting)
                            continue;

                        if (!_lastPackets.ContainsKey(peer.ID))
                            continue;

                        if (peer.Player.HasEscaped || !peer.Player.IsAlive)
                        {
                            _lastPackets[peer.ID] = 0;
                            continue;
                        }

                        if (_lastPackets[peer.ID] >= 4 * Ext.FRAMESPSEC)
                        {
                            server.DisconnectWithReason(server.GetSession(peer.ID), "AFK or Timeout");
                            continue;
                        }

                        _lastPackets[peer.ID] += Ext.FRAMESPSEC;
                    }
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
                case PacketType.IDENTITY:
                    {
                        Ext.HandleIdentity(server, session, reader);
                        break;
                    }

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
                            server.TCPMulticast(new TcpPacket(PacketType.SERVER_PLAYER_DEATH_STATE, session.ID, peer.Player.IsAlive, (byte)peer.Player.RevivalTimes));

                            lock (_reviveTimer)
                                _reviveTimer[peer.ID] = new();

                            server.TCPMulticast(new TcpPacket(PacketType.SERVER_REVIVAL_STATUS, false, session.ID));

                            if (!peer.Player.IsAlive)
                            {
                                Terminal.LogDiscord($"{peer.Nickname} died.");

                                if (peer.Player.DiedBefore || _map.Timer <= Ext.FRAMESPSEC * Ext.FRAMESPSEC * 2)
                                {
                                    var pkt = new TcpPacket(PacketType.SERVER_GAME_DEATHTIMER_END);

                                    if (_demonCount >= (int)((server.Peers.Count(e => !e.Value.Waiting) - 1) / 2.0))
                                        pkt.Write(0);
                                    else
                                    {
                                        peer.Player.RevivalTimes = 2;
                                        _demonCount++;

                                        Terminal.LogDiscord($"{peer.Nickname} was demonized!");

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

                            CheckEscapedAndAlive(server);
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
                            CheckEscapedAndAlive(server);

                            Terminal.LogDiscord($"{peer.Nickname} has escaped!");
                        }
                        break;
                    }

                case PacketType.CLIENT_REVIVAL_PROGRESS:
                    {
                        var rid = reader.ReadUInt16();
                        var rings = reader.ReadByte();

                        lock (server.Peers)
                            if (server.Peers[rid].Player.IsAlive || server.Peers[rid].Player.RevivalTimes >= 2)
                                break;

                        lock (_reviveTimer)
                        {
                            if (_reviveTimer[rid].Progress <= 0)
                                server.TCPMulticast(new TcpPacket(PacketType.SERVER_REVIVAL_STATUS, true, rid));

                            if (!_reviveTimer[rid].DeathNote.Contains(session.ID))
                                _reviveTimer[rid].DeathNote.Add(session.ID);

                            _reviveTimer[rid].Progress += 0.013 + 0.004 * rings;
                            if (_reviveTimer[rid].Progress > 1)
                            {
                                foreach (var p in _reviveTimer[rid].DeathNote)
                                    server.TCPSend(server.GetSession(p), new TcpPacket(PacketType.SERVER_REVIVAL_RINGSUB));

                                server.TCPMulticast(new TcpPacket(PacketType.SERVER_REVIVAL_STATUS, false, rid));
                                server.TCPSend(server.GetSession(rid), new TcpPacket(PacketType.SERVER_REVIVAL_REVIVED));

                                _reviveTimer[rid] = new();
                            }
                            else
                                server.UDPMulticast(ref IPEndPoints, new UdpPacket(PacketType.SERVER_REVIVAL_PROGRESS, rid, _reviveTimer[rid].Progress));
                        }
                        break;
                    }

                case PacketType.CLIENT_ERECTOR_BALLS:
                    {
                        var x = reader.ReadSingle();
                        var y = reader.ReadSingle();

                        server.TCPMulticast(new TcpPacket(PacketType.CLIENT_ERECTOR_BALLS, x, y));
                        break;
                    }
            }
        }

        private void UpdateDeathTimers(Server server)
        {
            lock (server.Peers)
            {
                var arr = server.Peers.Values.OrderBy(e => e.Player.DeadTimer);
                foreach (var peer in arr)
                {
                    if (peer.Waiting)
                        continue;

                    if (peer.Player.IsAlive || peer.Player.HasEscaped)
                    {
                        peer.Player.DeadTimer = -1;
                        continue;
                    }

                    if (peer.Player.DeadTimer == -1)
                        continue;

                    if ((int)peer.Player.DeadTimer % Ext.FRAMESPSEC == 0)
                    {
                        var pk = new TcpPacket(PacketType.SERVER_GAME_DEATHTIMER_TICK);
                        pk.Write((ushort)peer.ID);
                        pk.Write((byte)(peer.Player.DeadTimer / Ext.FRAMESPSEC));
                        server.TCPMulticast(pk);
                    }

                    peer.Player.DeadTimer -= (Ext.Dist(peer.Player.X, peer.Player.Y, server.Peers[_exeId].Player.X, server.Peers[_exeId].Player.Y) >= 240 ? 1f : 0.5f);

                    if (peer.Player.DeadTimer <= 0 || _map.Timer <= Ext.FRAMESPSEC * Ext.FRAMESPSEC * 2)
                    {
                        var pkt = new TcpPacket(PacketType.SERVER_GAME_DEATHTIMER_END);

                        if (_demonCount >= (int)((server.Peers.Count(e => !e.Value.Waiting) - 1) / 2.0))
                            pkt.Write(0);
                        else
                        {
                            _demonCount++;
                            peer.Player.RevivalTimes = 2;

                            Terminal.LogDiscord($"{peer.Nickname} was demonized!");
                            pkt.Write(1);
                        }

                        server.TCPSend(server.GetSession(peer.ID), pkt);
                        peer.Player.DeadTimer = -1;
                    }
                }
            }
        }

        private void CheckEscapedAndAlive(Server server)
        {
            lock (server.Peers)
            {
                if (_endTimer >= 0)
                    return;

                if (server.Peers.Count(e => !e.Value.Waiting) <= 0)
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
                    if (player.Value.Waiting)
                        continue;

                    if (player.Key == _exeId)
                        continue;

                    if (player.Value.Player.IsAlive)
                        alive++;

                    if (player.Value.Player.HasEscaped)
                        escaped++;
                }

                // Exe wins
                if (alive == 0 && escaped == 0)
                {
                    EndGame(server, 0);
                    return;
                }

                if ((server.Peers.Count(e => !e.Value.Waiting) - alive) + escaped >= server.Peers.Count(e => !e.Value.Waiting))
                {
                    if (escaped == 0)
                        EndGame(server, 0);
                    else
                        EndGame(server, 1);
                }
            }
        }

        public void EndGame(Server server, int type)
        {
            if (_endTimer >= 0)
                return;

            if (type == 0) // exe win
            {
                _endTimer = 5 * Ext.FRAMESPSEC;

                Terminal.LogDiscord($"Exe wins!");
                var pk = new TcpPacket(PacketType.SERVER_GAME_EXE_WINS);
                server.TCPMulticast(pk);
            }
            else if (type == 1) // survivors win
            {
                _endTimer = 5 * Ext.FRAMESPSEC;

                Terminal.LogDiscord($"Survivors win!");
                var pk = new TcpPacket(PacketType.SERVER_GAME_SURVIVOR_WIN);
                server.TCPMulticast(pk);
            }
            else if (type == 2) // time over
            {
                _endTimer = 5 * Ext.FRAMESPSEC;

                Terminal.LogDiscord($"Time over!");
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
