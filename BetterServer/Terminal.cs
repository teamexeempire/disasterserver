using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace BetterServer
{
    public enum LogLevel
    {
        Debug,
        Info
    }

    public class LogLine
    {
        public LogLevel Level;
        public string Message;
    }

    public class Terminal
    {
        public static List<LogLine> Buffer { get; private set; } = new();

        private static object _lock = new object();

        public static void Log(string text)
        {
            var time = DateTime.Now.ToLongTimeString();

            // For logging
            if (!Program.Config.EnableInput)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($"[{time} {Thread.CurrentThread.Name}] {text}");
                return;
            }

            lock (Buffer)
            {
                Buffer.Add(new LogLine()
                {
                    Level = LogLevel.Info,
                    Message = $"[{DateTime.Now.ToLongTimeString()} {Thread.CurrentThread.Name}] {text}"
                });

                if (Buffer.Count > 8192)
                    Buffer.RemoveAt(0);

                Redraw();
            }
        }

        public static void LogDebug(string text)
        {
            if (!Program.Config.LogDebug)
                return;

            var time = DateTime.Now.ToLongTimeString();

            // For logging
            if (!Program.Config.EnableInput)
            {
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine($"[{time} {Thread.CurrentThread.Name}] {text}");
                return;
            }

            lock (Buffer)
            {
                Buffer.Add(new LogLine()
                {
                    Level = LogLevel.Debug,
                    Message = $"[{DateTime.Now.ToLongTimeString()} {Thread.CurrentThread.Name}] {text}"
                });

                if (Buffer.Count > 8192)
                    Buffer.RemoveAt(0);

                Redraw();
            }
        }

        public static void LogDiscord(string text)
        {
            var time = DateTime.Now.ToLongTimeString();

            // For logging
            if (!Program.Config.EnableInput)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($"[{time} {Thread.CurrentThread.Name}] {text}");

                SendDiscord(text, Thread.CurrentThread.Name);
                return;
            }

            lock (Buffer)
            {
                Buffer.Add(new LogLine()
                {
                    Level = LogLevel.Info,
                    Message = $"[{DateTime.Now.ToLongTimeString()} {Thread.CurrentThread.Name}] {text}"
                });

                if (Buffer.Count > 8192)
                    Buffer.RemoveAt(0);

                Redraw();
            }

            SendDiscord(text, Thread.CurrentThread.Name);
        }

        public static void Redraw(int inputLen = -1)
        {
            lock (_lock)
            {
                // Store for later
                var pos = Console.CursorLeft;

                Console.CursorVisible = false;
                Console.SetCursorPosition(0, 0);
                Console.WriteLine("+--- Log Window --------------------+");
                Console.SetCursorPosition(0, 1);

                foreach (var line in Buffer.TakeLast(Console.BufferHeight - 5))
                {
                    Console.ForegroundColor = line.Level == LogLevel.Debug ? ConsoleColor.Gray : ConsoleColor.White;

                    int len = line.Message.Length > Console.BufferWidth ? Console.BufferWidth : line.Message.Length;
                    string msg = line.Message[..len];
                    string emptiness = new string(' ', Console.BufferWidth - line.Message[..len].Length);
                    Console.WriteLine($"{line.Message}{emptiness}");
                }

                Console.CursorVisible = true;
                Console.ForegroundColor = ConsoleColor.White;
                Console.SetCursorPosition(0, Console.BufferHeight - 3);
                Console.Write("+--- Command Window ----------------+");
                Console.SetCursorPosition(0, Console.BufferHeight - 2);

                if (inputLen != -1)
                {
                    var str = new string(' ', inputLen+1);
                    Console.Write(str);
                    Console.CursorLeft = 0;
                    Console.Write(":");
                }
                else
                {
                    Console.Write(":");
                    Console.CursorLeft = pos;
                }
            }
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
