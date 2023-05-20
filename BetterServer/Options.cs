using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BetterServer
{
    public class Options
    {
        private static JsonNode _doc;

        static Options()
        {
            try
            {
                if (!File.Exists("Config/Options.json"))
                    WriteDefault();
                else
                    _doc = JsonNode.Parse(File.ReadAllText("Config/Options.json"))!;
            }
            catch
            {
                Terminal.Log("Failed to load config.");
            }
        }

        private static void WriteDefault()
        {
            try
            {
                var @struct = new
                {
                    /* Shouldn't really be used but ok? */
                    server_count = 1,
                    webhook_url = "",
                    mapset_file = "", /* Console only */

                    enable_stat = false,
                    console_mode = false,
                    debug_mode = true
                };

                string ser = JsonSerializer.Serialize(@struct);
                _doc = JsonNode.Parse(ser)!;

                if (!Directory.Exists("Config"))
                    Directory.CreateDirectory("Config");

                File.WriteAllText("Config/Options.json", ser);
            }
            catch
            {
                Terminal.Log("Failed to save config.");
            } 
        }

        public static void Set(string key, dynamic value)
        {
            _doc[key] = value;

            File.WriteAllText("Config/Options.json", _doc.ToJsonString());
        }

        public static T? Get<T>(string key) => _doc[key].AsValue().Deserialize<T>();
    }
}
