using BetterServer.Data;
using BetterServer.Session;
using BetterServer.State;
using System.Text.Json;
using System.Text.Json.Nodes;

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
            Terminal.Log("Enter localhost or 127.0.0.1 on your PC to join the server.\n");

            // Load mlist from config
            // didnt test any of this bullshit should worj probably

            string? file = Options.Get<string>("mapset_file");

            if(!string.IsNullOrEmpty(file))
            {
                try
                {
                    JsonNode doc = JsonNode.Parse(File.ReadAllText(file))!;
                    JsonArray arr = doc.Root.AsArray();

                    foreach(JsonNode? node in arr)
                    {
                        if (node == null)
                            continue;

                        MapVote.Excluded.Add((int)node.AsValue());
                    }
                }
                catch (InvalidOperationException)
                {
                    Terminal.Log("Failed to load mapset_file (Invalid format?)!");
                }
                catch
                {
                    Terminal.Log("Failed to load mapset_file!");
                }
            }

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