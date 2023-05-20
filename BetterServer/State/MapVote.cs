using BetterServer.Data;
using BetterServer.Maps;
using BetterServer.Session;
using ExeNet;
using System.Net;

namespace BetterServer.State
{
    class MapVoteMap
    {
        public Map Map;
        public byte MapID;
        public int Votes;
    }

    public class MapVote : State
    {
        /* List of possible maps */
        public static readonly Type[] Maps = new Type[]
        {
            typeof(HideAndSeek2),
            typeof(RavineMist),
            typeof(DotDotDot),
            typeof(DesertTown),
            typeof(YouCantRun),
            typeof(LimpCity),
            typeof(NotPerfect),
            typeof(KindAndFair),
            typeof(Act9),
            typeof(NastyParadise),
            typeof(PricelessFreedom),
            typeof(VolcanoValley),
            typeof(GreenHill),
            typeof(MajinForest),
            typeof(AngelIsland),
            typeof(TortureCave),

            /* whaaar */
            typeof(FartZone)
        };

        public static List<int> Excluded = new() { 16 };

        private MapVoteMap[] _votes = new MapVoteMap[]
        {
            new(),
            new(),
            new(),
        };

        private int _timer = Ext.FRAMESPSEC;
        private int _timerSec = 30;
        private Random _rand = new();
        private Dictionary<ushort, bool> _votePeers = new();

        public override Session.State AsState()
        {
            return Session.State.VOTE;
        }

        public override void Init(Server server)
        {
            var numbers = new List<int>();
            var number = _rand.Next(0, Maps.Length);

            int uniqueCount = 0;

            for(int i = 0; i < Maps.Length; i++)
            {
                if (Excluded.Contains(i))
                    continue;

                uniqueCount++;
            }

            if(uniqueCount < 3)
                server.LastMap = -1;

            for (var i = 0; i < (uniqueCount >= _votes.Length ? _votes.Length : uniqueCount); i++)
            {
                while ((Excluded.Contains(number) || numbers.Contains(number) || number == server.LastMap))
                    number = _rand.Next(0, Maps.Length - 1);

                numbers.Add(number);
            }

            if(uniqueCount < _votes.Length)
            {
                for(int i = 0; i < _votes.Length - uniqueCount; i++)
                    numbers.Add(number);
            }

            for (var i = 0; i < numbers.Count; i++)
            {
                _votes[i].Map = Ext.CreateOfType<Map>(Maps[numbers[i]]) ?? new HideAndSeek2();
                _votes[i].MapID = (byte)numbers[i];
                _votes[i].Votes = 0;
            }

            lock (server.Peers)
            {
                foreach (var peer in server.Peers)
                {
                    if (peer.Value.Pending)
                    {
                        server.DisconnectWithReason(server.GetSession(peer.Key), "PacketType.CLIENT_REQUESTED_INFO missing.");
                        continue;
                    }

                    _votePeers.Add(peer.Key, false);
                }
            }

            var packet = new TcpPacket(PacketType.SERVER_VOTE_MAPS, _votes[0].MapID, _votes[1].MapID, _votes[2].MapID);
            server.TCPMulticast(packet);
        }

        public override void PeerJoined(Session.Server server, TcpSession session, Peer peer)
        {
        }

        public override void PeerLeft(Session.Server server, TcpSession session, Peer peer)
        {
            lock (server.Peers)
            {
                if (server.Peers.Count <= 1)
                {
                    server.SetState<Lobby>();
                    return;
                }

                lock (_votePeers)
                {
                    _votePeers.Remove(session.ID);

                    if (_votePeers.Count(e => !e.Value) <= 0)
                        CheckVotes(server);
                }
            }
        }

        public override void PeerTCPMessage(Session.Server server, TcpSession session, BinaryReader reader)
        {
            var passtrough = reader.ReadBoolean();
            var type = reader.ReadByte();

            if (passtrough)
                server.Passtrough(reader, session);

            switch ((PacketType)type)
            {
                case PacketType.CLIENT_VOTE_REQUEST:
                    {
                        var map = reader.ReadByte();

                        if (map >= _votes.Length)
                            break;

                        if (_votePeers[session.ID])
                            break;

                        lock (_votePeers)
                        {
                            _votePeers[session.ID] = true;

                            if (_votePeers.Count(e => !e.Value) <= 0)
                            {
                                if (_timerSec > 3)
                                {
                                    _timer = 1;
                                    _timerSec = 4;
                                }
                            }
                        }

                        _votes[map].Votes++;

                        var pkt = new TcpPacket(PacketType.SERVER_VOTE_SET, (byte)_votes[0].Votes, (byte)_votes[1].Votes, (byte)_votes[2].Votes);
                        server.TCPMulticast(pkt);
                        break;
                    }
            }
        }

        public override void PeerUDPMessage(Server server, IPEndPoint IPEndPoint, BinaryReader reader)
        {
        }

        public override void Tick(Server server)
        {
            if (_timer-- <= 0)
            {
                _timer = Ext.FRAMESPSEC;
                _timerSec--;

                var packet = new TcpPacket(PacketType.SERVER_VOTE_TIME_SYNC, (byte)_timerSec);
                server.TCPMulticast(packet);

                if (_timerSec <= 0)
                    CheckVotes(server);
            }
        }

        private void CheckVotes(Server server)
        {
            var max = _votes.Max(e => e.Votes);
            var votes = _votes.Where(e => e.Votes == max).ToArray();
            var map = votes[_rand.Next(0, votes.Length)];

            server.LastMap = map.MapID;
            server.SetState(new CharacterSelect(map.Map));
        }
    }
}
