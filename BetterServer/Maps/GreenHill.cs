using BetterServer.Entities;
using BetterServer.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetterServer.Maps
{
    public class GreenHill : Map
    {
        public override void Init(Server server)
        {
            SetTime(server, 180);
            Spawn<GHZThunder>(server);

            base.Init(server);
        }

        protected override int GetRingSpawnCount()
        {
            return 26;
        }
    }
}
