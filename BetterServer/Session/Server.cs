﻿using BetterServer.Data;
using BetterServer.Maps;
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

        /* Actual servers */
        public MulticastServer MulticastServer;
        public SharedServer SharedServer;

        private Thread? _thread;
        private int _hbTimer = 0;

        public Server(int uid)
        {
            MulticastServer = new(this, UDP_PORT + uid);
            SharedServer = new(this, TCP_PORT + uid);
            UID = uid+1;
        }

        public void StartAsync()
        {
            Logger.Log($"Starting...");

            if (!SharedServer.Start())
                throw new Exception("Failed to start SharedServer (TCP)");

            MulticastServer.Start();

            Logger.Log("Server is running");
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

            _thread.Name = $"Server {UID}";
            _thread.Start();
        }

        public void Tick() => State.Tick(this);
        public SharedServerSession? GetSession(ushort id) => (SharedServerSession?)SharedServer.GetSession(id);

        public bool TCPSend(TcpSession? session, TcpPacket packet)
        {
            if (session == null)
                return false;

            try
            {
                var arr = packet.ToArray();
                return session.Send(arr, packet.Length);
            }
            catch (Exception e)
            {
                Logger.LogDiscord($"Exception: {e}");
                return false;
            }
        }

        public bool TCPMulticast(TcpPacket packet, ushort? except = null)
        {
            bool success = true;

            try
            {
                var arr = packet.ToArray();

                lock (Peers)
                {
                    foreach (var peer in Peers)
                    {
                        if (peer.Value.Pending)
                            continue;

                        if (peer.Key == except)
                            continue;

                        var session = GetSession(peer.Value.ID);

                        if (session == null)
                            continue;

                        if (!session.Send(arr, packet.Length))
                            success = false;
                    }
                }

                return success;
            }
            catch (Exception e)
            {
                Logger.LogDiscord($"Exception: {e}");
                return false;
            }
        }

        public bool UDPSend(IPEndPoint IPEndPoint, UdpPacket packet)
        {
            try
            {
                var arr = packet.ToArray();
                var sent = MulticastServer.Send(IPEndPoint, arr, packet.Length);

                if (sent == -1)
                {
                    
                }

                return sent >= 0;
            }
            catch (InvalidOperationException)
            {
                return false;
            }
            catch (Exception e)
            {
                Logger.LogDiscord($"Exception: {e}");
                return false;
            }
        }

        public bool UDPMulticast(ref List<IPEndPoint> IPEndPoints, UdpPacket packet, IPEndPoint? except = null)
        {
            bool success = true;

            try
            {
                var arr = packet.ToArray();

                lock (IPEndPoints)
                {
                    foreach (var IPEndPoint in IPEndPoints)
                    {
                        if (IPEndPoint == except)
                            continue;

                        if (MulticastServer.Send(IPEndPoint, arr, packet.Length) <= 0)
                            success = false;

                        success = true;
                    }
                }
            }
            catch (InvalidOperationException)
            {
                return false;
            }
            catch (Exception e)
            {
                Logger.LogDiscord($"Exception: {e}");
                return false;
            }

            return success;
        }

        public bool UDPMulticast(ref Dictionary<ushort, IPEndPoint> IPEndPoints, UdpPacket packet, IPEndPoint? except = null)
        {
            bool success = true;

            try
            {
                var arr = packet.ToArray();

                lock (IPEndPoints)
                {
                    foreach (var IPEndPoint in IPEndPoints)
                    {
                        if (IPEndPoint.Value == except)
                            continue;

                        if (MulticastServer.Send(IPEndPoint.Value, arr, packet.Length) <= 0)
                            success = false;

                        success = true;
                    }
                }
            }
            catch (InvalidOperationException)
            {
                return false;
            }
            catch (Exception e)
            {
                Logger.LogDiscord($"Exception: {e}");
                return false;
            }

            return success;
        }

        public void Passtrough(BinaryReader reader, TcpSession sender)
        {
            Logger.LogDebug("Passtrough()");
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
            Logger.LogDebug("Passtrough end()");
        }

        public void DisconnectWithReason(TcpSession? session, string reason)
        {
            if (session == null)
                return;

            // Disconnection is handled on another thread since we don't want to deadlock server
            ThreadPool.QueueUserWorkItem((s) =>
            {
                Thread.CurrentThread.Name = $"Server {UID}";

                try
                {
                    var endpoint = session.RemoteEndPoint;
                    var id = session.ID;

                    lock (Peers)
                    {
                        if (!Peers.ContainsKey(id))
                        {
                            Logger.LogDiscord($"(ID {id}) disconnect: {reason}");
                        }
                        else
                        {

                            var peer = Peers[id];
                            Logger.LogDiscord($"{peer.Nickname} (ID {peer.ID}) disconnect: {reason}");
                        }
                    }

                    var pk = new TcpPacket(PacketType.SERVER_PLAYER_FORCE_DISCONNECT);
                    pk.Write(reason);
                    TCPSend(session, pk);
                    session.Disconnect();
                }
                catch { }
            });
        }

        public void SetState<T>() where T : BetterServer.State.State
        {
            var obj = Activator.CreateInstance(typeof(T));

            if (obj == null)
                return;

            State = (BetterServer.State.State)obj;
            State.Init(this);

            Logger.LogDiscord($"Server state is {State} now");
        }

        public void SetState<T>(T value) where T : BetterServer.State.State
        {
            State = value;
            State.Init(this);

            Logger.LogDiscord($"Server state is {State} now");
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

            Logger.LogDebug("Server heartbeated.");
            _hbTimer = 0;
        }
    }
}