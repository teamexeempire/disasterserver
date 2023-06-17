using BetterServer.Data;
using BetterServer.Maps;
using BetterServer.Session;
using BetterServer.State;

namespace BetterServer.Entities
{
    public class DTTailsDoll : Entity
    {
        private int _target = -1;
        private int _timer = 0;

        public override TcpPacket? Destroy(Server server, Game game, Map map)
        {
            return null;
        }

        public override TcpPacket? Spawn(Server server, Game game, Map map)
        {
            FindPost(server);
            return new TcpPacket(PacketType.SERVER_DTTAILSDOLL_STATE, (byte)0, (ushort)X, (ushort)Y, (_target == -1 && _timer <= 0));
        }

        public override UdpPacket? Tick(Server server, Game game, Map map)
        {
            if (_timer > 0)
            {
                if(_timer == 1)
                    server.TCPSend(server.GetSession((ushort)_target), new TcpPacket(PacketType.SERVER_DTTAILSDOLL_STATE, (byte)3));

                _timer--;
            }

            lock (server.Peers)
            {
                if (_timer <= 0 && (_target == -1 || !server.Peers.ContainsKey((ushort)_target)))
                {
                    foreach (var player in server.Peers.Values)
                    {
                        if (player.Player.Character == Character.Exe)
                            continue;

                        if (player.Player.RevivalTimes >= 2)
                            continue;

                        double dist = Dist(player.Player.X, player.Player.Y, X, Y);

                        if (dist < 130)
                        {
                            _target = player.ID;
                            _timer = Ext.FRAMESPSEC;

                            server.TCPSend(server.GetSession(player.ID), new TcpPacket(PacketType.SERVER_DTTAILSDOLL_STATE, (byte)2));
                            break;
                        }
                    }
                }

                if (_target != -1 && _timer <= 0)
                { 
                    foreach (var player in server.Peers.Values)
                    {
                        if (player.Player.Character == Character.Exe)
                            continue;

                        if (player.Player.RevivalTimes >= 2)
                            continue;

                        double dist = Dist(player.Player.X, player.Player.Y, X, Y);

                        if (dist < 80)
                        {
                            _target = player.ID;
                            break;
                        }
                    }

                    var plr = server.Peers[(ushort)_target].Player;
                    
                    if ((int)Math.Abs(plr.X - X) >= 4)
                        X += Math.Sign((int)plr.X - X) * 4;

                    if((int)Math.Abs(plr.Y - Y) >= 2)
                        Y += Math.Sign((int)plr.Y - Y) * 2;

                    if (Dist(plr.X, plr.Y, X, Y) < 18)
                    {
                        server.TCPSend(server.GetSession((ushort)_target), new TcpPacket(PacketType.SERVER_DTTAILSDOLL_STATE, (byte)1));
                        FindPost(server);

                        _target = -1;
                    }
                }
            }

            byte state = 0;

            if (_target == -1)
                state = 0;

            if (_target != -1 && _timer > 0)
                state = 1;

            if (_target != -1 && _timer <= 0)
                state = 2;

            return new UdpPacket(PacketType.SERVER_DTTAILSDOLL_STATE, (ushort)X, (ushort)Y, (byte)state);
        }

        private void FindPost(Server server)
        {
            Vector2[] pos = new Vector2[]
            {
                new Vector2(177, 944),
                new Vector2(1953, 544),
                new Vector2(3279, 224),
                new Vector2(4101, 544),
                new Vector2(4060, 1264),
                new Vector2(3805, 1824),
                new Vector2(2562, 1584),
                new Vector2(515, 1824),
                new Vector2(2115, 1056),
                new Vector2(984, 1184),
                new Vector2(1498, 1504),
            };

            List<Vector2> choosen = new();

            foreach (var p in pos)
                choosen.Add(p);

            lock (server.Peers)
            {
                foreach (var p in pos)
                {
                    foreach (var player in server.Peers.Values)
                    {
                        double dist = Ext.Dist(player.Player.X, player.Player.Y, p.X, p.Y);

                        if (dist < 480)
                        {
                            choosen.Remove(p);
                            break;
                        }
                    }
                }

                if(choosen.Count > 0)
                {
                    var point = choosen[new Random().Next(choosen.Count)];
                    X = point.X;
                    Y = point.Y;

                    Terminal.LogDebug($"Tails doll found spot at ({point.X}, {point.Y})");
                }
                else
                {
                    var point = pos[new Random().Next(choosen.Count)];
                    X = point.X;
                    Y = point.Y;

                    Terminal.LogDebug($"Tails doll didn't find a spot, using ({point.X}, {point.Y})");
                }
            }
        }
    }
}
