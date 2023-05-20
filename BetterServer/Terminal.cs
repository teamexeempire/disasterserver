using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace BetterServer
{
    public class Terminal
    {
        private static StringBuilder _builder = new();
        private static int _lines = 0;
        private static string _fname;

        static Terminal()
        {
            try
            {
                if (!Directory.Exists("Logs"))
                    Directory.CreateDirectory("Logs");

                _fname = $"Logs/{DateTime.Now:yyyyMMddTHHmmss}.log";
                AppDomain currentDomain = default(AppDomain);
                currentDomain = AppDomain.CurrentDomain;
                currentDomain.UnhandledException += CurrentDomain_UnhandledException;
                currentDomain.ProcessExit += CurrentDomain_ProcessExit;
            }
            catch
            {
                Console.WriteLine("Logging to file is disabled due to an error.");
            }
        }

        private static void CurrentDomain_ProcessExit(object? sender, EventArgs e)
        {
            try
            {
                File.AppendAllText(_fname, _builder.ToString());
            }
            catch
            { }
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                File.AppendAllText(_fname, _builder.ToString());
            }
            catch
            { }
        }

        public static void Log(string text)
        {
            var time = DateTime.Now.ToLongTimeString();
            var msg = $"[{time} {Thread.CurrentThread.Name} INFO] {text}";

            Console.ForegroundColor = ConsoleColor.White;
            Program.Window.Log(msg);

            lock (_builder)
            {
                _builder.AppendLine(msg);

                if (_lines++ > 20)
                {
                    File.AppendAllText(_fname, _builder.ToString());
                    _builder.Clear();
                }
            }
        }

        public static void LogDebug(string text)
        {
            var time = DateTime.Now.ToLongTimeString();
            var msg = $"[{time} {Thread.CurrentThread.Name} DEBUG] {text}";

            lock (_builder)
            {
                _builder.AppendLine(msg);

                if (_lines++ > 20)
                {
                    File.AppendAllText(_fname, _builder.ToString());
                    _builder.Clear();
                }
            }

            if (!Options.Get<bool>("debug_mode"))
                return;

            Console.ForegroundColor = ConsoleColor.Yellow;
            Program.Window.Log(msg);
        }

        public static void LogDiscord(string text)
        {
            var time = DateTime.Now.ToLongTimeString();
            var msg = $"[{time} {Thread.CurrentThread.Name} INFO] {text}";

            Console.ForegroundColor = ConsoleColor.White;
            Program.Window.Log(msg);

            lock (_builder)
            {
                _builder.AppendLine(msg);

                if (_lines++ > 20)
                {
                    File.AppendAllText(_fname, _builder.ToString());
                    _builder.Clear();
                }
            }

            if (string.IsNullOrEmpty(Options.Get<string>("webhook_url")))
                return;

            SendDiscord(text, Thread.CurrentThread.Name);
        }

        public static void SendDiscord(string message, string? name, string title = "")
        {
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
                    await client.PostAsync(Options.Get<string>("webhook_url")!, content);
                }
                catch
                { }
            });
        }
    }
}
