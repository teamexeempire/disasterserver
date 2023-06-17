using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetterServer
{
    public static class Ext
    {
        public const int FRAMESPSEC = 60;

        public static string ReadStringNull(this BinaryReader reader)
        {
            List<byte> bytes = new List<byte>();

            byte c;
            while (reader.BaseStream.Position < reader.BaseStream.Length && (c = reader.ReadByte()) != (byte)'\0')
                bytes.Add(c);

            return Encoding.UTF8.GetString(bytes.ToArray());
        }

        public static T? CreateOfType<T>()
        {
            return (T?)Activator.CreateInstance(typeof(T));
        }

        public static T? CreateOfType<T>(Type value)
        {
            return (T?)Activator.CreateInstance(value);
        }

        public static double Dist(double x, double y, double x2, double y2)
        {
            return Math.Sqrt(Math.Pow(x2 - x, 2) + Math.Pow(y2 - y, 2));
        }
    }
}
