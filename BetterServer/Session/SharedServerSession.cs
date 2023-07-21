using BetterServer.Data;
using ExeNet;
using System.Net;
using System.Net.Sockets;

namespace BetterServer.Session
{
    public class SharedServerSession : TcpSession
    {
        private Server _server;

        private List<byte> _header = new();
        private List<byte> _data = new();
        private int _length = -1;
        private bool _start = false;

        private byte[] _headerData = new byte[] { (byte)'h', (byte)'P', (byte)'K', (byte)'T', (byte)0x0 };

        public SharedServerSession(Server server, TcpClient client) : base(server.SharedServer, client)
        {
            _server = server;
        }

        protected override void OnConnected()
        {
            Thread.CurrentThread.Name = $"Server {_server.UID}";

            lock (_server.Peers)
            {
                if (KickList.Check((RemoteEndPoint! as IPEndPoint).Address.ToString()!))
                {
                    _server.DisconnectWithReason(this, "Kicked by server.");
                    return;
                }

                if (BanList.Check((RemoteEndPoint! as IPEndPoint).Address.ToString()!))
                {
                    _server.DisconnectWithReason(this, "You were banned from this server.");
                    return;
                }

                if (_server.Peers.Count >= 7)
                {
                    _server.DisconnectWithReason(this, "Server is full. (7/7)");
                    return;
                }

                var peer = new Peer()
                {
                    EndPoint = RemoteEndPoint!,

                    Player = new(),
                    ID = ID,
                    Pending = true,
                    Waiting = _server.State.AsState() != State.LOBBY
                };
                _server.Peers.Add(ID, peer);
                
                if(!peer.Waiting)
                    _server.State.PeerJoined(_server, this, peer);

                Terminal.Log($"{RemoteEndPoint} (ID {ID}) connected.");
            }

            base.OnConnected();
        }

        protected override void OnDisconnected()
        {
            Thread.CurrentThread.Name = $"Server {_server.UID}";

            lock (_server.Peers)
            {
                _server.Peers.Remove(ID, out Peer? peer);

                if (peer == null)
                    return;

                Program.Window.RemovePlayer(peer);
                if (peer.Waiting)
                {
                    Terminal.Log($"{peer?.EndPoint} (waiting) (ID {peer?.ID}) disconnected.");
                    return;
                }

                var packet = new TcpPacket(PacketType.SERVER_PLAYER_LEFT, peer.ID);
                _server.TCPMulticast(packet, ID);

                _server.State.PeerLeft(_server, this, peer);
                Terminal.Log($"{peer?.EndPoint} (ID {peer?.ID}) disconnected.");
            }

            Program.Stat?.MulticastInformation();
            base.OnDisconnected();
        }

        protected override void OnData(byte[] buffer, int length)
        {
            Thread.CurrentThread.Name = $"Server {_server.UID}";
            ProcessBytes(buffer, length);

            base.OnData(buffer, length);
        }

        protected override void Timeouted()
        {
            _server.DisconnectWithReason(this, "Connection timeout");
            base.Timeouted();
        }

        private void ProcessBytes(byte[] buffer, int length)
        {
            using var memStream = new MemoryStream(buffer, 0, length);

            while (memStream.Position < memStream.Length)
            {
                byte bt = (byte)memStream.ReadByte();

                if (_start)
                {
                    _start = false;
                    _length = bt;
                    _data.Clear();

                    Terminal.LogDebug($"Packet start {_length}");
                }
                else
                {
                    _data.Add(bt);

                    if (_data.Count >= _length && _length != -1)
                    {
                        var data = _data.ToArray();
                        using var stream = new MemoryStream(data);
                        using var reader = new BinaryReader(stream);

                        Terminal.LogDebug($"Packet recv {BitConverter.ToString(data)}");

                        try
                        {
                            if (data.Length > 256)
                            {
                                Terminal.LogDiscord("TCP overload (data.Length > 256)");
                                _server.DisconnectWithReason(this, "Packet overload > 256");
                            }
                            else
                                _server.State.PeerTCPMessage(_server, this, reader);
                        }
                        catch (Exception e)
                        {
                            OnError(e.Message);
                        }
                        _length = -1;
                        _data.Clear();
                    }
                }

                _header.Add(bt);
                if (_header.Count >= 6)
                    _header.RemoveAt(0);

                // Now check header
                if (Enumerable.SequenceEqual(_header, _headerData))
                    _start = true;
            }

            if (_data.Count < _length && _length != -1)
                Terminal.LogDebug($"Packet split, waiting for part to arrive.");
        }

        protected override void OnSocketError(SocketError error)
        {
            Thread.CurrentThread.Name = $"Server {_server.UID}";
            _server.State.TCPSocketError(this, error);

            base.OnSocketError(error);
        }

        protected override void OnError(string message)
        {
            Thread.CurrentThread.Name = $"Server {_server.UID}";
            Terminal.LogDiscord($"Caught Error: {message}");

            base.OnError(message);
        }
    }
}
