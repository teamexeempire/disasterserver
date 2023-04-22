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
}
