using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetterServer.Data
{
    public class Player
    {
        public Character Character = Character.None;
        public int RevivalTimes = 0;
        public int DeadTimer = -1;
        public bool DiedBefore = false;
        public bool HasEscaped = false;
        public bool IsReady = false;
        public bool IsAlive = true;
        public bool IsHurt = false;
        public bool HasRedRing = false;
        public bool CanDemonize = false;
    }
}
