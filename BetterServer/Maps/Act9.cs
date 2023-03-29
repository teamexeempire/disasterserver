using BetterServer.Entities;
using BetterServer.Session;

namespace BetterServer.Maps
{
    public class Act9 : Map
    {
        public override void Init(Server server)
        {
            SetTimer(server, 130); //130
            ActivateRingAfter(20);

            Spawn(server, new Act9Wall(0, 0, 1025));
            Spawn(server, new Act9Wall(1, 1663, 0));
            Spawn(server, new Act9Wall(2, 1663, 0));

            base.Init(server);
        }

        public override void Tick(Server server)
        {
            base.Tick(server);
        }
    }
}
