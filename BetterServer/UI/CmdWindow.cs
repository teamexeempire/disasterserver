using BetterServer.Data;
using BetterServer.Session;
using BetterServer.State;

namespace BetterServer.UI
{
    public class CmdWindow : Window
    {
        public override void Log(string message)
        {
            Console.WriteLine(message);
        }

        public override bool Run()
        {
#if _WINDOWS
            UIWrapper.AllocConsole();
#endif

            Terminal.Log("===================");
            Terminal.Log("TD2DR Server");
            Terminal.Log($"BUILD v{Program.BUILD_VER}");
            Terminal.Log("(c) Team Exe Empire 2023");
            Terminal.Log("===================");
            Terminal.Log("Enter 127.0.0.1 on your PC to join your server.\n");

            for (var i = 0; i < Options.Get<int>("server_count"); i++)
            {
                Server server = new(i);
                server.StartAsync();
                Program.Servers.Add(server);
            }

            while (true) ;
        }
    }
}