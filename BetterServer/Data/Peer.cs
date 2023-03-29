﻿using System.Net;

namespace BetterServer.Data
{
    public class Peer
    {
        public ushort ID = 0;
        public string Nickname = "Pending...";

        public bool Pending = true;

        public EndPoint EndPoint;
        public Player Player;
    }
}
