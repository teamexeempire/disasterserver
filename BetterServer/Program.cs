using BetterServer.Session;
using BetterServer.State;
using BetterServer.UI;
using System.Text.Json;

namespace BetterServer
{
    public class Program
    {
        public const int BUILD_VER = 320;
        public const int MAX_PLAYERS = 7;
        public static List<Server> Servers { get; private set; } = new();
        public static Window Window { get; private set; }

        public static void Main(string[] args)
        {
            if (Options.Get<int>("server_count") <= 0)
            {
#if _WINDOWS
                UIWrapper.AllocConsole();
#endif
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine("ServerCount is set to 0 in config.");
                return;
            }

            if (Options.Get<bool>("enable_stat"))
            {
                //TODO: make stat server
                //StatServer stat = new();
                //stat.Start();
            }

            Window = Options.Get<bool>("console_mode") || Options.Get<int>("server_count") > 1 ? new CmdWindow() : new MainWindow();
            if (!Window.Run())
            {
                Window = new CmdWindow();
                Window.Run();
            }
        }

        public static Server? FindServer(string id)
        {
            if (!int.TryParse(id, out int srvId))
            {
                return null;
            }

            srvId--;

            if (srvId < 0 || srvId >= Servers.Count)
                return null;

            return Servers[srvId];
        }
    }
}