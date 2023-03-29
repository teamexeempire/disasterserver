using BetterServer.Entities;
using BetterServer.Session;

namespace BetterServer.Maps
{
    public class NotPefect : Map
    {
        public override void Init(Server server)
        {
            Spawn<NotPerfectController>(server);
            SetTimer(server, 155);

            base.Init(server);
        }

        public override void Tick(Server server)
        {
            base.Tick(server);
        }
    }
}
