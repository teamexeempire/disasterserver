using BetterServer.UI;
using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace BetterServer
{
    public class BanList
    {
        private static JsonNode _doc;

        static BanList()
        {
            try
            {
                if (!File.Exists("Config/Banlist.json"))
                    WriteDefault();
                else
                    _doc = JsonNode.Parse(File.ReadAllText("Config/Banlist.json"))!;
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
                string ser = JsonSerializer.Serialize(new { });
                _doc = JsonNode.Parse(ser)!;

                if (!Directory.Exists("Config"))
                    Directory.CreateDirectory("Config");

                File.WriteAllText("Config/Banlist.json", ser);
            }
            catch
            {
                Terminal.Log("Failed to save banlist.");
            }
        }

        public static bool Ban(ushort pid, out string nickname, out string ip)
        {
            nickname = "";
            ip = "";

            foreach (var server in Program.Servers)
            {
                lock (server.Peers)
                {
                    if (!server.Peers.ContainsKey(pid))
                        continue;

                    nickname = server.Peers[pid].Nickname;
                    ip = (server.Peers[pid].EndPoint as IPEndPoint).Address.ToString();

                    _doc[ip] = nickname;
                    File.WriteAllText("Config/Banlist.json", _doc.ToJsonString());
                    return true;
                }
            }

            return false;    
        }

        public static void Unban(string ip)
        {
            if (!_doc.AsObject().ContainsKey(ip))
                return;

            _doc.AsObject().Remove(ip);
            File.WriteAllText("Config/Banlist.json", _doc.ToJsonString());
        }

        public static bool Check(string ip)
        {
            if (!_doc.AsObject().ContainsKey(ip))
                return false;

            return true;
        }

        public static List<KeyValuePair<string, string>> GetBanned()
        {
            List<KeyValuePair<string, string>> strings = new();

            foreach(var it in _doc.AsObject())
            {
                var val = it.Value;
                
                if (val == null) 
                    continue;

                strings.Add(new KeyValuePair<string, string>(it.Key, val.GetValue<string>()));
            }

            return strings;
        }
    }
}
