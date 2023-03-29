using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BetterServer
{
    public class Logger
    {
        public static void Log(string text)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"[{DateTime.Now.ToLongTimeString()} {Thread.CurrentThread.Name}] {text}");
        }

        public static void LogDebug(string text)
        {
            if (!Program.Config.LogDebug)
                return;

            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine($"[{DateTime.Now.ToLongTimeString()} {Thread.CurrentThread.Name} (DEBUG)] {text}");
        }

        public static void LogDiscord(string text)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"[{DateTime.Now.ToLongTimeString()} {Thread.CurrentThread.Name}] {text}");
            SendDiscord(text, Thread.CurrentThread.Name);
        }

        public static void SendDiscord(string message, string? name, string title = "")
        {
            if (Program.Config?.WebhookURL == null)
                return;

            var @struct = new
            {
                username = $"Thread ({name})",
                embeds = new List<object>
                {
                    new
                    {
                        title = title,
                        description = $"``` {message} ```"
                    }
                }
            };

            var json = JsonSerializer.Serialize(@struct);
            ThreadPool.QueueUserWorkItem(async (re) =>
            {
                try
                {
                    var client = new HttpClient();
                    client.Timeout = TimeSpan.FromSeconds(2);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    await client.PostAsync(Program.Config?.WebhookURL, content);
                }
                catch
                { }
            });
        }
    }
}
