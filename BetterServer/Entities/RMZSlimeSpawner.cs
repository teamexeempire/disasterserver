using BetterServer.Data;
using BetterServer.Maps;
using BetterServer.Session;
using BetterServer.State;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetterServer.Entities
{
    internal class RMZSlimeSpawner : Entity
    {
        public int ID = -1;
        public bool HasSlime = false;
        public RMZSlime Slime;

        public const int SPAWN_INTERVAL = (15 * Ext.FRAMESPSEC);

        private int _timer = SPAWN_INTERVAL;
        private Random _rand = new();
        private static byte _slimeIds = 0;
        private object _lock = new object();

        public RMZSlimeSpawner(int x, int y)
        {
            X = x;
            Y = y;
        }

        public override TcpPacket? Spawn(Server server, Game game, Map map)
        {
            lock(_lock)
                _timer += _rand.Next(2, 17) * Ext.FRAMESPSEC;

            return null;
        }

        public override TcpPacket? Destroy(Server server, Game game, Map map)
        {
            return null;
        }

        public override UdpPacket? Tick(Server server, Game game, Map map)
        {
            lock (_lock)
            {
                if (HasSlime)
                    return null;

                if (_timer-- > 0)
                    return null;

                Slime = map.Spawn(server, new RMZSlime()
                {
                    X = X,
                    Y = Y,
                    SpawnX = X,
                    SpawnY = Y,
                    ID = _slimeIds++
                });

                HasSlime = true;
                ID = Slime.ID;
            }
            return null;
        }

        public void KillSlime(Server server, Map map, Peer killer, bool isProjectile)
        {
            lock (_lock)
            {
                if (!HasSlime)
                    return;

                switch (Slime.State)
                {
                    // Ring
                    case 2:
                    case 3:
                        {
                            if (isProjectile)
                                break;

                            var sess = server.GetSession(killer.ID);
                            server.TCPSend(sess, new TcpPacket
                            (
                                PacketType.SERVER_RMZSLIME_RINGBONUS,

                                false
                            ));

                        }
                        break;

                    // Red ring
                    case 4:
                    case 5:
                        {
                            if (isProjectile)
                                break;

                            var sess = server.GetSession(killer.ID);
                            server.TCPSend(sess, new TcpPacket
                            (
                                PacketType.SERVER_RMZSLIME_RINGBONUS,

                                true
                            ));
                        }
                        break;
                }

                map.Destroy(server, Slime);
                HasSlime = false;
                _timer = SPAWN_INTERVAL + _rand.Next(2, 17) * Ext.FRAMESPSEC;
                ID = -1;
            }
        }
    }
}
