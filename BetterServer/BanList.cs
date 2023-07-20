using BetterServer.UI;
using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace BetterServer
{
    class Ban
    {
        [JsonPropertyName("list")]
        public Dictionary<string, Dictionary<string, string>> List = new();
    }

    public class BanList
    {
        private static Ban _list = new();

        static BanList()
        {
            try
            {
                if (!File.Exists("Config/Banlist.json"))
                    WriteDefault();
                else
                    _list = JsonSerializer.Deserialize<Ban>(File.ReadAllText("Config/Banlist.json"))!;
            }
            catch
            {
                Terminal.Log("Failed to load banlist.");
            }
        }

        public static void WriteDefault()
        {
            try
            {
                string ser = JsonSerializer.Serialize(_list);

                if (!Directory.Exists("Config"))
                    Directory.CreateDirectory("Config");

                File.WriteAllText("Config/Banlist.json", ser);
            }
            catch
            {
                Terminal.Log("Failed to save banlist.");
            }
        }

        public static bool Ban(ushort pid, out string nickname, out string unique)
        {
            unique = "";
            nickname = "";

            foreach (var server in Program.Servers)
            {
                lock (server.Peers)
                {
                    if (!server.Peers.ContainsKey(pid))
                        continue;

                    unique = server.Peers[pid].Unique;
                    nickname = server.Peers[pid].Nickname;
                    var ip = (server.Peers[pid].EndPoint as IPEndPoint).Address.ToString();

                    var @struct = new Dictionary<string, string>()
                    {
                        { "name", nickname },
                        { "ip", ip }
                    };

                    _list.List[unique] = @struct;
                    File.WriteAllText("Config/Banlist.json", JsonSerializer.Serialize(_list));
                    return true;
                }
            }

            return false;    
        }

        public static void Unban(string unqie)
        {
            if (!_list.List.ContainsKey(unqie))
                return;

            _list.List.Remove(unqie);
            File.WriteAllText("Config/Banlist.json", JsonSerializer.Serialize(_list));
        }

        public static bool Check(string unqie)
        {
            if (!_list.List.ContainsKey(unqie))
                return false;

            return true;
        }

        public static Dictionary<string, Dictionary<string, string>> GetBanned()
        {
            return _list.List;
        }
    }
}
