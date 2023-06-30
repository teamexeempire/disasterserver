using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetterServer
{
    public class FastBitReader
    {
        public int Position { get; set; } = 0;

        public byte ReadByte(ref byte[] data)
        {
            if (Position >= data.Length)
                throw new ArgumentOutOfRangeException("data");

            return data[Position++];
        }

        public bool ReadBoolean(ref byte[] data)
        {
            if (Position >= data.Length)
                throw new ArgumentOutOfRangeException("data");

            return Convert.ToBoolean(data[Position++]);
        }

        public char ReadChar(ref byte[] data)
        {
            if (Position >= data.Length)
                throw new ArgumentOutOfRangeException("data");

            return (char)data[Position++];
        }

        public short ReadShort(ref byte[] data)
        {
            if (Position >= data.Length)
                throw new ArgumentOutOfRangeException("data");

            short val = (short)(data[Position] | ((uint)data[Position + 1] << 8));

            Position += 2;
            return val;
        }

        public ushort ReadUShort(ref byte[] data)
        {
            if (Position >= data.Length)
                throw new ArgumentOutOfRangeException("data");

            ushort val = (ushort)(data[Position] | ((uint)data[Position + 1] << 8));

            Position += 2;
            return val;
        }

        public int ReadInt(ref byte[] data)
        {
            if (Position >= data.Length)
                throw new ArgumentOutOfRangeException("data");

            int val = (int)(data[Position] | ((uint)data[Position + 1] << 8) | ((uint)data[Position + 2] << 16) | ((uint)data[Position + 3] << 24));

            Position += 4;
            return val;
        }

        public uint ReadUInt(ref byte[] data)
        {
            if (Position >= data.Length)
                throw new ArgumentOutOfRangeException("data");

            uint val = (uint)(data[Position] | ((uint)data[Position + 1] << 8) | ((uint)data[Position + 2] << 16) | ((uint)data[Position + 3] << 24));

            Position += 4;
            return val;
        }

        public unsafe float ReadFloat(ref byte[] data)
        {
            if (Position >= data.Length)
                throw new ArgumentOutOfRangeException("data");

            float val = 0;
            fixed (byte* v = &data[Position])
                val = *(float*)v;

            Position += 4;
            return val;
        }

        public long ReadLong(ref byte[] data)
        {
            if (Position >= data.Length)
                throw new ArgumentOutOfRangeException("data");

            long val = (long)(data[Position] | ((ulong)data[Position + 1] << 8) | ((ulong)data[Position + 2] << 16) | ((ulong)data[Position + 3] << 24) | ((ulong)data[Position + 4] << 32) | ((ulong)data[Position + 5] << 40 | ((ulong)data[Position + 6] << 48 | ((ulong)data[Position + 7] << 56))));

            Position += 8;
            return val;
        }

        public ulong ReadULong(ref byte[] data)
        {
            if (Position >= data.Length)
                throw new ArgumentOutOfRangeException("data");

            ulong val = (ulong)(data[Position] | ((ulong)data[Position + 1] << 8) | ((ulong)data[Position + 2] << 16) | ((ulong)data[Position + 3] << 24) | ((ulong)data[Position + 4] << 32) | ((ulong)data[Position + 5] << 40 | ((ulong)data[Position + 6] << 48 | ((ulong)data[Position + 7] << 56))));

            Position += 8;
            return val;
        }
    }
}
