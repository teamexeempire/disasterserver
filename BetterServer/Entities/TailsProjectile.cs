using BetterServer.Maps;
using BetterServer.Session;
using BetterServer.State;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetterServer.Entities
{
    internal class TailsProjectile : Entity
    {
        public sbyte Direction;
        public bool IsExe;
        public byte Charge;
        public byte Damage;

        private int _timer = 5 * Ext.FRAMESPSEC;

        public override TcpPacket? Spawn(Server server, Game game, Map map)
        {
            return new TcpPacket
            (
                PacketType.SERVER_TPROJECTILE_STATE,

                (byte)0, // Spawn
                (ushort)X,
                (ushort)Y,
                Direction,
                Damage,
                IsExe,
                Charge
            );
        }

        public override TcpPacket? Destroy(Server server, Game game, Map map)
        {
            return new TcpPacket
            (
                PacketType.SERVER_TPROJECTILE_STATE,

                (byte)2 // Destroy
            );
        }

        public override UdpPacket? Tick(Server server, Game game, Map map)
        {
            X += Direction * 12;

            if(X <= 0 || _timer-- <= 0)
                map.Destroy(server, this);

            return new UdpPacket
            (
                PacketType.SERVER_TPROJECTILE_STATE,

                (byte)1, // Update
                (ushort)X,
                (ushort)Y,
                Direction,
                Damage,
                IsExe,
                Charge
            );
        }
    }
}
