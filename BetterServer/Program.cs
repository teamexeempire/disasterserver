using BetterServer;
using BetterServer.Data;
using BetterServer.Maps;
using BetterServer.Session;
using System.Text.Json;

public class Program
{
    public const int BUILD_VER = 303; 
    public const int MAX_PLAYERS = 7;

    public static Config Config { get; private set; }
    public static List<Server> Servers = new();

    public static void Main(string[] args)
    {
        Logger.Log("=============================");
        Logger.Log(" SeTD2R Server ");
        Logger.Log($" BUILD v{BUILD_VER}");
        Logger.Log(" (c) 2023 Team Exe Empire ");
        Logger.Log("=============================");

        if (!LoadConfig(args.Length > 0 ? args[0] : "ServerConfig.json"))
            return;

        Console.WriteLine();
        for (var i = 0; i < Config?.ServerCount; i++)
        {
            Server server = new(i);
            server.StartAsync();
            Servers.Add(server);
        }

        if(Config.EnableStat)
        {
            //TODO: make stat server
            //StatServer stat = new();
            //stat.Start();
        }

        while (true)
            Thread.Sleep(200);
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
        catch(Exception ex)
        {
            Logger.Log($"Failed to load config: {ex}");
            Console.ReadLine();
            return false;
        }

        Logger.Log($"Loaded config from \"{fname}\"");
        return true;
    }
}