using BetterServer.Data;
using BetterServer.Entities;
using BetterServer.Session;
using BetterServer.State;
using ExeNet;
using System.Net;

namespace BetterServer.Maps
{
    public abstract class Map
    {
        public Game Game;

        public int Timer = 2 * Ext.FRAMESPSEC * Ext.FRAMESPSEC;
        public List<Entity> Entities = new();
        public bool BigRingSpawned = false;
        public bool BigRingReady = false;
        public ushort RingIDs = 1;
        public ushort BRingsIDs = 1;
        public byte ExellerCloneIDs = 1;

        protected int RingActivateTime = (Ext.FRAMESPSEC * Ext.FRAMESPSEC) - (10 * Ext.FRAMESPSEC);
        private float _ringCoff;
        private int _ringTimer = -(Ext.FRAMESPSEC * 4);
        private Random _rand = new();
        private bool[] _ringSpawns;

        public virtual void Init(Server server)
        {
            _ringCoff = GetRingTime();
            lock (server.Peers)
            {
                if (server.Peers.Count > 3)
                    _ringCoff--;
            }

            _ringSpawns = new bool[GetRingSpawnCount()];

            var pack = new TcpPacket(PacketType.SERVER_GAME_PLAYERS_READY);
            server.TCPMulticast(pack);
        }

        public virtual void Tick(Server server)
        {
            if (Timer % Ext.FRAMESPSEC == 0)
            {
                var packet = new TcpPacket(PacketType.SERVER_GAME_TIME_SYNC);
                packet.Write((ushort)(Timer));
                server.TCPMulticast(packet);
            }

            DoRingTimer(server);
            DoBigRingTimer(server);

            if (Timer > 0)
                Timer--;

            lock (Entities)
            {
                for (var i = 0; i < Entities.Count; i++)
                {
                    var ent = Entities[i];
                    var packet = ent.Tick(server, Game, this);

                    if (packet == null)
                        continue;

                    server.UDPMulticast(ref Game.IPEndPoints, packet);
                }
            }
        }

        public virtual void PeerLeft(Server server, TcpSession session, Peer peer)
        {
        }

        public virtual void PeerUDPMessage(Server server, IPEndPoint endpoint, BinaryReader reader)
        {
        }

        public virtual void PeerTCPMessage(Server server, TcpSession session, BinaryReader reader)
        {
            reader.BaseStream.Seek(0, SeekOrigin.Begin);
            var passtrough = reader.ReadBoolean();
            var type = reader.ReadByte();

            switch ((PacketType)type)
            {
                /* Tails' projectile */
                case PacketType.CLIENT_TPROJECTILE:
                    {
                        if (FindOfType<TailsProjectile>().Length > 0)
                            break;

                        var projectile = new TailsProjectile
                        {
                            OwnerID = session.ID,
                            X = reader.ReadUInt16(),
                            Y = reader.ReadUInt16(),
                            Direction = reader.ReadSByte(),
                            Damage = reader.ReadByte(),
                            IsExe = reader.ReadBoolean(),
                            Charge = reader.ReadByte()
                        };

                        Spawn(server, projectile);
                        break;
                    }

                case PacketType.CLIENT_TPROJECTILE_HIT:
                    {
                        Destroy<TailsProjectile>(server);
                        break;
                    }

                case PacketType.CLIENT_ETRACKER:
                    {
                        var tracker = new EggmanTracker
                        {
                            X = reader.ReadUInt16(),
                            Y = reader.ReadUInt16()
                        };

                        Spawn(server, tracker);
                        break;
                    }

                case PacketType.CLIENT_ETRACKER_ACTIVATED:
                    {
                        lock (Entities)
                        {
                            var id = reader.ReadByte();
                            var activator = reader.ReadUInt16();

                            var trackers = FindOfType<EggmanTracker>();
                            if (trackers == null)
                                break;

                            var tracker = trackers.Where(e => e.ID == id).FirstOrDefault();
                            if (tracker == null)
                                break;

                            tracker.ActivatorID = activator;
                            Destroy(server, tracker);
                        }
                        break;
                    }

                case PacketType.CLIENT_CREAM_SPAWN_RINGS:
                    {
                        var _x = reader.ReadUInt16();
                        var _y = reader.ReadUInt16();
                        var _cnt = reader.ReadByte();
                        var _redRing = reader.ReadBoolean();

                        for (var i = 0; i < _cnt; i++)
                        {
                            if (_redRing)
                            {
                                Spawn(server, new CreamRing()
                                {
                                    X = (int)(_x + Math.Sin(Math.PI * 2.5 - (i * Math.PI)) * 26),
                                    Y = (int)(_y + Math.Cos(Math.PI * 2.5 - (i * Math.PI)) * 26),
                                    IsRedRing = true
                                });
                            }
                            else
                            {

                                Spawn(server, new CreamRing()
                                {
                                    X = (int)(_x + Math.Sin(Math.PI * 2.5 + (i * (Math.PI / 3))) * 26),
                                    Y = (int)(_y + Math.Cos(Math.PI * 2.5 + (i * (Math.PI / 3))) * 26),
                                    IsRedRing = false
                                });
                            }
                        }

                        break;
                    }

                case PacketType.CLIENT_RING_COLLECTED:
                    {
                        lock (Entities)
                        {
                            var id = reader.ReadByte();
                            var uid = reader.ReadUInt16();

                            var rings = FindOfType<Ring>();
                            if (rings == null)
                                break;

                            var ring = rings.Where(e => e.ID == uid).FirstOrDefault();
                            if (ring == null)
                                break;

                            var packet = new TcpPacket(PacketType.SERVER_RING_COLLECTED, ring.IsRedRing);
                            server.TCPSend(session, packet);

                            Destroy(server, ring);
                        }
                        break;
                    }

                case PacketType.CLIENT_BRING_COLLECTED:
                    {
                        lock (Entities)
                        {
                            var uid = reader.ReadUInt16();

                            var rings = FindOfType<BlackRing>();
                            if (rings == null)
                                break;

                            var ring = rings.Where(e => e.ID == uid).FirstOrDefault();
                            if (ring == null)
                                break;

                            var packet = new TcpPacket(PacketType.SERVER_BRING_COLLECTED);
                            server.TCPSend(session, packet);

                            Destroy(server, ring);
                        }
                        break;
                    }

                case PacketType.CLIENT_ERECTOR_BRING_SPAWN:
                    {
                        var x = reader.ReadSingle();
                        var y = reader.ReadSingle();
                        var inst = Spawn<BlackRing>(server, false);
                        inst.ID = BRingsIDs++;

                        server.TCPMulticast(new TcpPacket(PacketType.SERVER_ERECTOR_BRING_SPAWN, inst.ID, x, y));
                        break;
                    }

                case PacketType.CLIENT_EXELLER_SPAWN_CLONE:
                    {
                        if (FindOfType<ExellerClone>().Length > 1)
                            break;

                        var x = reader.ReadUInt16();
                        var y = reader.ReadUInt16();
                        var dir = reader.ReadSByte();
                        Spawn(server, new ExellerClone(session.ID, ExellerCloneIDs++, x, y, dir));
                        break;
                    }

                case PacketType.CLIENT_EXELLER_TELEPORT_CLONE:
                    {
                        var uid = reader.ReadByte();
                        var clones = FindOfType<ExellerClone>();

                        if (clones == null)
                            break;

                        var clone = clones.Where(e => e.ID == uid).FirstOrDefault();
                        if (clone == null)
                            break;  

                        Destroy(server, clone);
                        break;
                    }
            }
        }

        public void SetTime(Server server, int seconds)
        {
            Timer = (seconds * Ext.FRAMESPSEC) + (GetPlayerOffset(server) * Ext.FRAMESPSEC);
            Terminal.LogDebug($"Timer is set to {Timer} frames");
        }

        public void ActivateRingAfter(int afterSeconds)
        {
            RingActivateTime = (Ext.FRAMESPSEC * Ext.FRAMESPSEC) - (afterSeconds * Ext.FRAMESPSEC);
            Terminal.LogDebug($"Ring activate time is set to {Timer} frames");
        }

        public void FreeRingID(byte iid)
        {
            lock (_ringSpawns)
                _ringSpawns[iid] = false;
        }

        public bool GetFreeRingID(out byte iid)
        {
            lock (_ringSpawns)
            {
                if (_ringSpawns.Where(e => !e).Count() <= 0)
                {
                    iid = 0;
                    return false;
                }

                while (true)
                {
                    byte rn = (byte)_rand.Next(_ringSpawns.Length);

                    if (_ringSpawns[rn])
                        continue;

                    _ringSpawns[rn] = true;
                    iid = rn;
                    return true;
                }
            }
        }

        #region Entities

        public T? Spawn<T>(Server server, bool callSpawn = true) where T : Entity
        {
            T? entity = Ext.CreateOfType<T>();

            if (entity == null)
                return null;

            TcpPacket? pack = null;

            lock (Entities)
            {
                Entities.Add(entity);

                if (callSpawn)
                    pack = entity.Spawn(server, Game, this);
            }

            if (pack != null)
                server.TCPMulticast(pack);

            Terminal.LogDebug($"Entity {entity} spawned.");
            return entity;
        }

        public T Spawn<T>(Server server, T entity, bool callSpawn = true) where T : Entity
        {
            TcpPacket? pack = null;

            lock (Entities)
            {
                Entities.Add(entity);

                if (callSpawn)
                    pack = entity.Spawn(server, Game, this);
            }

            if (pack != null)
                server.TCPMulticast(pack);

            Terminal.LogDebug($"Entity {entity} spawned.");
            return entity;
        }

        public void Destroy(Server server, Entity entity)
        {
            TcpPacket? pack;
            lock (Entities)
            {
                Entities.Remove(entity);
                pack = entity.Destroy(server, Game, this);
            }

            if (pack != null)
                server.TCPMulticast(pack);

            Terminal.LogDebug($"Entity {entity} destroyed.");
        }

        public void Destroy<T>(Server server) where T : Entity
        {
            lock (Entities)
            {
                var pick = FindOfType<T>();

                if (pick == null)
                    return;

                foreach (var p in pick)
                {
                    Destroy(server, p);
                    Terminal.LogDebug($"Entity {p} destroyed.");
                }
            }
        }

        public T[]? FindOfType<T>() where T : Entity
        {
            lock (Entities)
            {
                var pick = Entities.Where(e => e is T).ToArray();
                Terminal.LogDebug($"Entity search found {pick.Length} entities of type {typeof(T).FullName}");
                return Array.ConvertAll(pick, e => (T)e);
            }
        }

        #endregion

        private void DoRingTimer(Server server)
        {
            if (_ringTimer >= (_ringCoff * Ext.FRAMESPSEC))
            {
                _ringTimer = 0;

                if (!GetFreeRingID(out byte iid))
                    return;

                Spawn(server, new Ring()
                {
                    IID = iid
                });
            }
            _ringTimer++;
        }

        protected virtual void DoBigRingTimer(Server server)
        {
            if (Timer - (Ext.FRAMESPSEC * Ext.FRAMESPSEC) <= 0 && !BigRingSpawned)
            {
                var packet = new TcpPacket(PacketType.SERVER_GAME_SPAWN_RING);
                packet.Write(false);
                packet.Write((byte)_rand.Next(255));
                server.TCPMulticast(packet);

                BigRingSpawned = true;
            }

            var min = RingActivateTime; // 1 min - 10 sec
            if (Timer - min <= 0 && !BigRingReady)
            {
                var packet = new TcpPacket(PacketType.SERVER_GAME_SPAWN_RING);
                packet.Write(true);
                packet.Write((byte)_rand.Next(255));
                server.TCPMulticast(packet);

                BigRingSpawned = true;
            }
        }

        protected virtual int GetPlayerOffset(Server server)
        {
            lock (server.Peers)
                return (server.Peers.Count - 1) * 20;
        }

        protected virtual float GetRingTime()
        {
            return 5.0f;
        }

        protected abstract int GetRingSpawnCount();
    }
}
