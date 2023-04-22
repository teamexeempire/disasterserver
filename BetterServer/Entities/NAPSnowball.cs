using BetterServer.Maps;
using BetterServer.Session;
using BetterServer.State;

namespace BetterServer.Entities
{
    public class NAPSnowball : Entity
    {
        public byte ID = 0;

        private bool _active;
        private double _frame;
        private byte _stage;
        private sbyte _dir;
        private double _stateProg;
        private double _accel = 0;

        private float[] _waypoints;
        private float[] _waypointsSpeeds;

        private const int NUM_FRAMES = 32;
        private const int ROLL_START = 16;

        public NAPSnowball(byte nid, byte waypointCount, sbyte dir)
        {
            _accel = 0;
            _active = false;
            _frame = 0;
            _stage = 0;
            _stateProg = 0;
            _dir = dir;
            ID = nid;

            _waypoints = new float[waypointCount];
            Array.Fill(_waypoints, 0.05f);

            _waypointsSpeeds = new float[waypointCount];
            Array.Fill(_waypointsSpeeds, 0.35f);
        }

        public override TcpPacket? Spawn(Server server, Game game, Map map)
        {
            return null;
        }

        public override TcpPacket? Destroy(Server server, Game game, Map map)
        {
            return null;
        }

        public override UdpPacket? Tick(Server server, Game game, Map map)
        {
            if (!_active)
                return null;

            if (_accel > 1)
            {
                _frame += _waypointsSpeeds[_stage];

                if (_frame >= NUM_FRAMES)
                    _frame = ROLL_START;

                _stateProg += _waypoints[_stage];
            }
            else
            {
                Terminal.LogDebug($"{_accel}");
                _accel += 0.016;
                _frame += _accel * 0.45f;
                _stateProg += _accel * 0.05f;
            }

            if (_stateProg > 1)
            {
                _stateProg = 0;
                _stage++;

                if (_stage >= _waypoints.Length-1)
                {
                    _active = false;
                    _stage = 0;
                    _frame = 0;
                    _stateProg = 0;

                    server.TCPMulticast(new TcpPacket
                    (
                        PacketType.SERVER_NAPBALL_STATE,
                        (byte)2,
                        (byte)ID
                    ));

                    return null;
                }
            }

            return new UdpPacket
            (
                PacketType.SERVER_NAPBALL_STATE,
                (byte)1,
                (byte)ID,
                (byte)_stage,
                (byte)_frame,
                _stateProg
            );
        }

        public void Activate(Server server)
        {
            if (_active)
                return;

            _accel = 0;
            _stage = 0;
            _frame = 0;
            _stateProg = 0;
            _active = true;

            server.TCPMulticast(new TcpPacket
            (
                PacketType.SERVER_NAPBALL_STATE,
                (byte)0,
                (byte)ID,
                (sbyte)_dir
            ));
        }

        public void SetWaypointMoveSpeed(byte index, float speed) => _waypoints[index] = speed;
        public void SetWaypointAnimSpeed(byte index, float speed) => _waypointsSpeeds[index] = speed;
    }
}
