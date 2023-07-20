using System.Net;

namespace BetterServer.Data
{
    public class Peer
    {
        public ushort ID = 0;
        public string Nickname = "Pending...";
        public string Unique = "";
        public int ExeChance = 0;
        public byte Icon = 0;
        public sbyte Pet = -1;

        public bool Pending = true;
        public bool Waiting = false;

        public EndPoint EndPoint;
        public Player Player;
    }
}
