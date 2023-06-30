using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Timer = System.Timers.Timer;

namespace BetterServer
{
    struct KickData
    {
        public string IP;
        public DateTime Since;
    }

    public class KickList
    {
        private static List<KickData> EndPoints = new();
        private static Timer timer = new();

        static KickList()
        {
            timer.Interval = 1000 * 30;
            timer.Elapsed += Timer_Elapsed;
            timer.Start();
        }

        private static void Timer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            EndPoints.RemoveAll((e) => (DateTime.Now - e.Since).TotalMinutes >= 1);
        }

        public static void Add(string endpoint)
        {
            EndPoints.Add(new KickData()
            {
                IP = endpoint,
                Since = DateTime.Now
            });
        }
        
        public static bool Check(string endpoint)
        {
            return EndPoints.Any(e => e.IP == endpoint);
        }
    }
}
