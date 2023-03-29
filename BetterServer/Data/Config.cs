using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BetterServer.Data
{
    public class Config
    {
        [JsonPropertyName("WebhookURL")]
        public string? WebhookURL { get; set; } = null;

        [JsonPropertyName("ServerCount")]
        public int ServerCount { get; set; } = 1;

        [JsonPropertyName("LogDebug")]
        public bool LogDebug { get; set; } = true;

        [JsonPropertyName("EnableStat")]
        public bool EnableStat { get; set; } = false;

        [JsonPropertyName("BannedIPs")]
        public string[] BannedIPs { get; set; }
    }
}
