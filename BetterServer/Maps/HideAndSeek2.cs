using BetterServer.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetterServer.Maps
{
    public class HideAndSeek2 : Map
    {
        public override void Init(Server server)
        {
            SetTime(server, 205);
            base.Init(server);
        }

        public override void Tick(Server server)
        {
            base.Tick(server);
        }

        protected override int GetRingSpawnCount()
        {
            return 22;
        }
    }
}
