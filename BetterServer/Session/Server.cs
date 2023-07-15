using BetterServer.Data;
using BetterServer.State;
using ExeNet;
using System.Net;

namespace BetterServer.Session
{
    public class Server
    {
        private const int TCP_PORT = 7606;
        private const int UDP_PORT = 8606;

        /* Status */
        public int UID = -1;
        public bool IsRunning = false;
        public BetterServer.State.State State = new Lobby();

        /* Data */
        public Dictionary<ushort, Peer> Peers = new();
        public int LastMap = -1;

        /* Actual servers */
        public MulticastServer MulticastServer;
        public SharedServer SharedServer;

        private Thread? _thread;
        private int _hbTimer = 0;

        public Server(int uid)
        {
            MulticastServer = new(this, UDP_PORT + uid);
            SharedServer = new(this, TCP_PORT + uid);
            UID = uid + 1;
        }

        public void StartAsync()
        {
            if (!SharedServer.Start())
                throw new Exception("Failed to start SharedServer (TCP)");

            if (!MulticastServer.Start())
                throw new Exception("Failed to start MulticastServer (UCP)");

            IsRunning = true;

            _thread = new Thread(() =>
            {
                while (IsRunning)
                {
                    DoHeartbeat();
                    Tick();

                    Thread.Sleep(15);
                }
            });

            _thread.Priority = ThreadPriority.AboveNormal;
            _thread.Name = $"Server {UID}";
            _thread.Start();
        }

        public void Tick() => State.Tick(this);
        public SharedServerSession? GetSession(ushort id) => (SharedServerSession?)SharedServer.GetSession(id);

        public void TCPSend(TcpSession? session, TcpPacket packet)
        {
            if (session == null)
                return;

            try
            {
                var arr = packet.ToArray();
                session.Send(arr, packet.Length);
            }
            catch (Exception e)
            {
                Terminal.LogDiscord($"TCPSend() Exception: {e}");
            }
        }

        public void TCPMulticast(TcpPacket packet, ushort? except = null)
        {
            try
            {
                var arr = packet.ToArray();

                lock (SharedServer.Sessions)
                {
                    lock (Peers)
                    {
                        foreach (var peer in Peers)
                        {
                            if (peer.Key == except)
                                continue;

                            var session = GetSession(peer.Value.ID);

                            if (session == null)
                                continue;

                            session.Send(arr, packet.Length);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Terminal.LogDiscord($"TCPMulticast() Exception: {e}");
            }
        }

        public void UDPSend(IPEndPoint IPEndPoint, UdpPacket packet)
        {
            try
            {
                var arr = packet.ToArray();
                MulticastServer.Send(IPEndPoint, ref arr, packet.Length);
            }
            catch (InvalidOperationException)
            {
            }
            catch (Exception e)
            {
                Terminal.LogDiscord($"UDPSend() Exception: {e}");
            }
        }

        public void UDPMulticast(ref List<IPEndPoint> IPEndPoints, UdpPacket packet, IPEndPoint? except = null)
        {
            try
            {
                var arr = packet.ToArray();

                lock (IPEndPoints)
                {
                    foreach (var IPEndPoint in IPEndPoints)
                    {
                        if (IPEndPoint == except)
                            continue;

                        MulticastServer.Send(IPEndPoint, ref arr, packet.Length);
                    }
                }
            }
            catch (InvalidOperationException)
            {
            }
            catch (Exception e)
            {
                Terminal.LogDiscord($"UDPMulticast() Exception: {e}");
            }
        }

        public void UDPMulticast(ref Dictionary<ushort, IPEndPoint> IPEndPoints, UdpPacket packet, IPEndPoint? except = null)
        {
            try
            {
                var arr = packet.ToArray();

                lock (IPEndPoints)
                {
                    foreach (var IPEndPoint in IPEndPoints)
                    {
                        if (IPEndPoint.Value == except)
                            continue;

                        MulticastServer.Send(IPEndPoint.Value, ref arr, packet.Length);
                    }
                }
            }
            catch (InvalidOperationException)
            {
            }
            catch (Exception e)
            {
                Terminal.LogDiscord($"UDPMulticast() Exception: {e}");
            }
        }

        public void Passtrough(BinaryReader reader, TcpSession sender)
        {
            Terminal.LogDebug("Passtrough()");
            // remember pos
            var pos = reader.BaseStream.Position;
            reader.BaseStream.Seek(0, SeekOrigin.Begin);

            // Now find type
            _ = reader.ReadByte();
            var type = reader.ReadByte();

            var pk = new TcpPacket((PacketType)type);

            //now write
            while (reader.BaseStream.Position < reader.BaseStream.Length)
                pk.Write(reader.ReadByte());

            TCPMulticast(pk, sender.ID);

            //now return back
            reader.BaseStream.Seek(pos, SeekOrigin.Begin);
            Terminal.LogDebug("Passtrough end()");
        }

        public void DisconnectWithReason(TcpSession? session, string reason)
        {
            if (session == null)
                return;

            if (!session.IsRunning)
                return;

            Thread.CurrentThread.Name = $"Server {UID}";
            try
            {
                var endpoint = session.RemoteEndPoint;
                var id = session.ID;

                lock (Peers)
                {
                    if (!Peers.ContainsKey(id))
                    {
                        Terminal.LogDiscord($"(ID {id}) disconnect: {reason}");
                    }
                    else
                    {

                        var peer = Peers[id];
                        Terminal.LogDiscord($"{peer.Nickname} (ID {peer.ID}) disconnect: {reason}");
                    }
                }

                var pk = new TcpPacket(PacketType.SERVER_PLAYER_FORCE_DISCONNECT);
                pk.Write(reason);
                TCPSend(session, pk);
                session.Disconnect();
            }
            catch (Exception e) { Terminal.LogDiscord($"Disconnect failed: {e}"); }
        }

        public void SetState<T>() where T : BetterServer.State.State
        {
            var obj = Activator.CreateInstance(typeof(T));

            if (obj == null)
                return;

            State = (BetterServer.State.State)obj;
            State.Init(this);

            Terminal.LogDiscord($"Server state is {State} now");
        }

        public void SetState<T>(T value) where T : BetterServer.State.State
        {
            State = value;
            State.Init(this);

            Terminal.LogDiscord($"Server state is {State} now");
        }

        private void DoHeartbeat()
        {
            lock (Peers) /* Ignore if no peers */
            {
                if (Peers.Count <= 0)
                    return;
            }

            if (_hbTimer++ < 2 * Ext.FRAMESPSEC)
                return;

            var pk = new TcpPacket(PacketType.SERVER_HEARTBEAT);
            TCPMulticast(pk);

            Terminal.LogDebug("Server heartbeated.");
            _hbTimer = 0;
        }
    }
}
