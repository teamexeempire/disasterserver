using BetterServer.Data;
using BetterServer.Entities;
using BetterServer.Session;
using ExeNet;
using System.Net;

namespace BetterServer.Maps
{
    public class RavineMist : Map
    {
        private readonly Vector2[] _slimeSpawnPoints = new[]
        {
            new Vector2 (1901, 392),
            new Vector2 (2193, 392),
            new Vector2 (2468, 392),
            new Vector2 (1188, 860),
            new Vector2 (2577, 1952),
            new Vector2 (2564, 2264),
            new Vector2 (2782, 2264),
            new Vector2 (1441, 2264),
            new Vector2 (884, 2264),
            new Vector2 (988, 2004),
            new Vector2 (915, 2004),
        };

        private readonly Vector2[] _shardSpawnPoints = new[]
        {
            new Vector2 (862, 248),
            new Vector2 (3078, 248),
            new Vector2 (292, 558),
            new Vector2 (2918, 558),
            new Vector2 (1100, 820),
            new Vector2 (980, 1188),
            new Vector2 (1870, 1252),
            new Vector2 (2180, 1508),
            new Vector2 (2920, 2216),
            new Vector2 (282, 2228),
            new Vector2 (1318, 1916),
            new Vector2 (3010, 1766)
        };

        private Dictionary<ushort, byte> _playersShardCount = new();
        private Dictionary<ushort, Vector2> _playerPos = new();
        private Random _rand = new();
        private int _ringLoc = 0;

        public override void Init(Server server)
        {
            var points = _shardSpawnPoints.OrderBy(e => _rand.Next()).Take(7);

            lock (Entities)
            {
                foreach (var point in points)
                    Spawn(server, new RMZShard(point.X, point.Y));

                foreach (var coord in _slimeSpawnPoints)
                    Spawn(server, new RMZSlimeSpawner(coord.X, coord.Y));
            }

            lock (server.Peers)
            {
                foreach (var peer in server.Peers)
                {
                    _playerPos.Add(peer.Key, new());
                    _playersShardCount.Add(peer.Key, 0);
                }
            }

            _ringLoc = _rand.Next(255);

            SetTime(server, 180);
            base.Init(server);
        }

        public override void Tick(Server server)
        {
            base.Tick(server);
        }

        public override void PeerLeft(Server server, TcpSession session, Peer peer)
        {
            lock (_playersShardCount)
            {
                lock (_playerPos)
                {
                    lock (Entities)
                    {
                        for (var i = 0; i < _playersShardCount[session.ID]; i++)
                            Spawn(server, new RMZShard(_playerPos[session.ID].X + _rand.Next(-8, 8), _playerPos[session.ID].Y, true));
                    }

                    _playersShardCount.Remove(session.ID);
                    _playerPos.Remove(session.ID);
                }
            }

            SendRingState(server);
            base.PeerLeft(server, session, peer);
        }

        public override void PeerTCPMessage(Server server, TcpSession session, BinaryReader reader)
        {
            var passtrough = reader.ReadBoolean();
            var type = reader.ReadByte();

            switch ((PacketType)type)
            {
                case PacketType.CLIENT_PLAYER_DEATH_STATE:
                    {
                        lock (Entities)
                        {
                            var isAlive = !reader.ReadBoolean();
                            var rT = reader.ReadByte();

                            if (!isAlive)
                            {
                                lock (_playersShardCount)
                                {
                                    lock (_playerPos)
                                    {
                                        for (var i = 0; i < _playersShardCount[session.ID]; i++)
                                            Spawn(server, new RMZShard(_playerPos[session.ID].X + _rand.Next(-8, 8), _playerPos[session.ID].Y, true));

                                        _playersShardCount[session.ID] = 0;
                                    }
                                }

                                SendRingState(server);
                            }
                        }
                        break;
                    }

                case PacketType.CLIENT_RMZSLIME_HIT:
                    {
                        lock (Entities)
                        {
                            var id = reader.ReadByte();
                            var pid = reader.ReadUInt16();
                            var isProj = reader.ReadBoolean();

                            Terminal.Log($"Killing slime {id}");

                            var slimes = FindOfType<RMZSlimeSpawner>();
                            if (slimes == null)
                                break;

                            var slime = slimes.Where(e => e.ID == id).FirstOrDefault();
                            if (slime == null)
                                break;

                            Peer? killer;
                            lock (server.Peers)
                                killer = server.Peers.Values.Where(e => e.ID == pid).FirstOrDefault();

                            if (killer == null)
                                break;

                            slime.KillSlime(server, this, killer, isProj);
                            Terminal.Log($"Killed slime {id}");
                            break;
                        }
                    }

                case PacketType.CLIENT_RMZSHARD_COLLECT:
                    {
                        lock (Entities)
                        {
                            var gid = reader.ReadByte();
                            var list = FindOfType<RMZShard>();

                            if (list == null)
                                return;

                            var ent = list.FirstOrDefault(e => e.ID == gid);

                            if (ent == null)
                                return;

                            lock (_shardSpawnPoints)
                                _playersShardCount[session.ID]++;

                            server.TCPMulticast(new TcpPacket(PacketType.SERVER_RMZSHARD_STATE, (byte)2, (byte)ent.ID, session.ID));

                            Destroy(server, ent);
                            SendRingState(server);
                        }
                        break;
                    }
            }

            base.PeerTCPMessage(server, session, reader);
        }

        public override void PeerUDPMessage(Server server, IPEndPoint endpoint, BinaryReader reader)
        {
            var pid = reader.ReadUInt16();

            switch ((PacketType)reader.ReadByte())
            {
                case PacketType.CLIENT_PLAYER_DATA:
                    pid = reader.ReadUInt16();
                    float x = reader.ReadSingle();
                    float y = reader.ReadSingle();

                    lock (_playerPos)
                    {
                        _playerPos[pid].X = (int)x;
                        _playerPos[pid].Y = (int)y;
                    }
                    break;
            }

            base.PeerUDPMessage(server, endpoint, reader);
        }

        protected override void DoBigRingTimer(Server server)
        {
            if (Timer - (Ext.FRAMESPSEC * Ext.FRAMESPSEC) <= 0 && !BigRingSpawned)
            {
                var packet = new TcpPacket(PacketType.SERVER_GAME_SPAWN_RING);
                packet.Write(false);
                packet.Write((byte)_ringLoc);
                server.TCPMulticast(packet);

                BigRingSpawned = true;
            }

            if (Timer - RingActivateTime <= 0 && !BigRingReady)
            {
                lock (Entities)
                {
                    var count = 7 - Entities.Count(e => e is RMZShard);
                    var packet = new TcpPacket(PacketType.SERVER_GAME_SPAWN_RING);
                    packet.Write(count >= 6);
                    packet.Write((byte)_ringLoc);
                    server.TCPMulticast(packet);

                    BigRingSpawned = true;
                }
            }
        }

        public void SendRingState(Server server)
        {
            lock (Entities)
            {
                var count = 7 - Entities.Count(e => e is RMZShard);
                server.TCPMulticast(new TcpPacket(PacketType.SERVER_RMZSHARD_STATE, (byte)3, (byte)count));

                if (Timer - RingActivateTime <= 0 && !BigRingReady)
                {
                    var packet = new TcpPacket(PacketType.SERVER_GAME_SPAWN_RING);
                    packet.Write(count >= 6);
                    packet.Write((byte)_ringLoc);
                    server.TCPMulticast(packet);

                    BigRingSpawned = true;
                }
            }
        }

        protected override int GetRingSpawnCount()
        {
            return 27;
        }
    }
}
