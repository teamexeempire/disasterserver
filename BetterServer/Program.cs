using BetterServer.Data;
using BetterServer.Session;
using System.Text.Json;

namespace BetterServer
{
    public class Program
    {
        public const int BUILD_VER = 312;
        public const int MAX_PLAYERS = 7;

        public static Config Config { get; private set; }
        public static List<Server> Servers = new();

        public static void Main(string[] args)
        {
            if (!LoadConfig(args.Length > 0 ? args[0] : "ServerConfig.json"))
                return;

            Terminal.Log("=============================");
            Terminal.Log(" SeTD2R Server ");
            Terminal.Log($" BUILD v{BUILD_VER}");
            Terminal.Log(" (c) 2023 Team Exe Empire ");
            Terminal.Log("=============================\n");

            if (Config.ServerCount <= 0)
            {
                Terminal.Log("Baka why did you set it to zero???");
                return;
            }

            Console.WriteLine();
            for (var i = 0; i < Config.ServerCount; i++)
            {
                Server server = new(i);
                server.StartAsync();
                Servers.Add(server);
            }

            Terminal.Log("To join the game on your pc, enter 127.0.0.1 as IP Addreess");

            if (Config.EnableStat)
            {
                //TODO: make stat server
                //StatServer stat = new();
                //stat.Start();
            }

            ProccessCommands();
        }

        private static void ProccessCommands()
        {
            if (!Config.EnableInput)
                while (true) Thread.Sleep(250);

            var searchMode = false;
            var searchInd = 0;
            LogLine[]? searchResults = null;

            Console.CursorLeft++;
            while (true)
            {
                var command = Console.ReadLine();
                var cmds = command.Split(' ');

                Console.SetCursorPosition(0, Console.BufferHeight - 1);
                var str = new string(' ', Console.BufferWidth - 1);
                Console.Write(str);
                Console.CursorLeft = 0;

                if (searchMode)
                {
                    switch (cmds[0])
                    {
                        case "next":
                            {
                                searchInd++;

                                if (searchInd >= searchResults.Length)
                                {
                                    Console.Write("Reached end. Type \"exit\" to exit searching mode.");
                                    break;
                                }

                                var result = searchResults[searchInd];
                                Console.Write($"({searchInd + 1}/{searchResults.Length}) {result.Message}");
                                break;
                            }

                        case "prev":
                            {
                                searchInd--;

                                if (searchInd < 0)
                                {
                                    Console.Write("Reached beginning. Type \"exit\" to exit searching mode.");
                                    break;
                                }

                                var result = searchResults[searchInd];
                                Console.Write($"({searchInd+1}/{searchResults.Length}) {result.Message}");
                                break;
                            }

                        case "exit":
                            searchMode = false;
                            break;

                        default:
                            {
                                lock (Terminal.Buffer)
                                    searchResults = Terminal.Buffer.Where(e => e.Message.Contains(cmds[0])).ToArray();

                                searchInd = 0;
                                if (searchResults.Length <= 0)
                                {
                                    Console.Write("No results found.");
                                    break;
                                }

                                var result = searchResults[searchInd];
                                Console.Write($"({searchInd + 1}/{searchResults.Length}) {result.Message}");
                                break;
                            }
                    }

                    Terminal.Redraw(command.Length);
                    continue;
                }

                switch (cmds[0])
                {
                    case "help":
                        Console.Write("help say search kick ban quit");
                        break;

                    case "search":
                        searchMode = true;
                        Console.Write("You are in search mode now, enter the keyword. Navigate using \"next\", \"prev\" and \"exit\"");
                        break;

                    case "say":
                        if (cmds.Length > 2)
                        {
                            var srv = FindServer(cmds[1]);

                            if (srv == null)
                            {
                                Console.Write("Server with this id doesn't exist.");
                                break;
                            }

                            string message = string.Join(' ', cmds.TakeLast(cmds.Length - 2));

                            var packet = new TcpPacket(PacketType.CLIENT_CHAT_MESSAGE);
                            packet.Write((ushort)0);
                            packet.Write(message);

                            srv.TCPMulticast(packet);

                            Terminal.LogDiscord($"[SERVER]: {message}");
                            break;
                        }

                        Console.Write("say [server id] [message]");
                        break;

                    case "exit":
                    case "quit":
                        Environment.Exit(0);
                        break;

                    case "exewin":
                        if (cmds.Length > 1)
                        {
                            var srv = FindServer(cmds[1]);

                            if (srv == null)
                            {
                                Console.Write("Server with this id doesn't exist.");
                                break;
                            }

                            if (srv.State is State.Game)
                                (srv.State as State.Game).EndGame(srv, 0);
                            break;
                        }

                        Console.Write("exewin [server id]");
                        break;

                    default:
                        Console.Write("Unknown command. See !help");
                        break;
                }

                Terminal.Redraw(command.Length);
            }
        }

        private static Server? FindServer(string id)
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

        private static bool LoadConfig(string fname)
        {
            try
            {
                string text = File.ReadAllText(fname);
                var conf = JsonSerializer.Deserialize<Config>(text);
                ArgumentNullException.ThrowIfNull(conf);

                Config = conf;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load config: {ex}");
                Console.ReadLine();
                return false;
            }

            return true;
        }
    }
}