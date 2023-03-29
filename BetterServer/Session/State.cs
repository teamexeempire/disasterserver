using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetterServer.Session
{
    public enum State : byte
    {
        LOBBY,
        CHARACTERSELECT,
        GAME
    }
}
