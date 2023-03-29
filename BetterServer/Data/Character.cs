using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetterServer.Data
{
    public enum Character : int
    {
        NONE = -1,

        EXE = 0,
        TAILS = 1,
        KNUCKLES = TAILS + 1,
        EGGMAN = TAILS + 2,
        AMY = TAILS + 3,
        CREAM = TAILS + 4,
        SALLY = TAILS + 5,
    }
}
