using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetterServer.Data
{
    public enum Character : int
    {
        None = -1,

        Exe = 0,
        Tails = 1,
        Knuckles = Tails + 1,
        Eggman = Tails + 2,
        Amy = Tails + 3,
        Cream = Tails + 4,
        Sally = Tails + 5,
    }

    public enum ExeCharacter
    {
        None = -1,
        Original,
        Chaos,
        Exetior,
        Exeller
    }

    public class Player
    {
        public Character Character = Character.None;
        public ExeCharacter ExeCharacter = ExeCharacter.None;

        public int RevivalTimes = 0;
        public int DeadTimer = -1;
        public bool DiedBefore = false;
        public bool HasEscaped = false;
        public bool IsReady = false;
        public bool IsAlive = true;
        public bool IsHurt = false;
        public bool HasRedRing = false;
        public bool CanDemonize = false;
        public bool Invisible = false;

        public double X, Y;
    }
}
