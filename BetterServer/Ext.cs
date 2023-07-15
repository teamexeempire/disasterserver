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
        private static Random _rand = new();

        public static string ReadStringNull(this BinaryReader reader)
        {
            List<byte> bytes = new();

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

        public static string ValidateNick(string nick)
        {
            string nick2 = nick;
            char[] pattern = new char[] { '\\', '/', '@', '|', '№', '`', '~', '&', ' ' };

            foreach(var ch in pattern)
                nick2 = nick2.Replace(ch.ToString(), "");

            if (nick2.Length <= 0 || string.IsNullOrEmpty(nick2) || string.IsNullOrWhiteSpace(nick2))
                return $"/player~ \\{_rand.Next(9999)}";

            return nick;
        }
    }
}
