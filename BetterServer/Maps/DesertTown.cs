using BetterServer.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetterServer.Maps
{
    public class DesertTown : Map
    {
        public override void Init(Server server)
        {
            SetTimer(server, 180);
            base.Init(server);
        }

        public override void Tick(Server server)
        {
            base.Tick(server);
        }
    }
}
