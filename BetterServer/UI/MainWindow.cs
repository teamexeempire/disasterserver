using BetterServer.Data;
using BetterServer.Session;
using BetterServer.State;
using System.Security.Cryptography;

namespace BetterServer.UI
{
    public class MainWindow : Window
    {
        public override void AddPlayer(Peer peer)
        {
            UIWrapper.PlayerData data = new()
            {
                state = 0,
                pid = peer.ID,
                name = peer.Nickname,
                character = peer.Player.Character == Character.Exe ? UIWrapper.PlayerCharacter.CHARACTER_SALLY + (int)peer.Player.ExeCharacter : (UIWrapper.PlayerCharacter)peer.Player.Character
            };

            UIWrapper.gui_player_state(data);
        }

        public override void Log(string message)
        {
            UIWrapper.gui_log(message);
        }

        public override void RemovePlayer(Peer peer)
        {
            UIWrapper.PlayerData data = new()
            {
                state = 1,
                pid = peer.ID,
                name = peer.Nickname,
                character = peer.Player.Character == Character.Exe ? UIWrapper.PlayerCharacter.CHARACTER_SALLY + (int)peer.Player.ExeCharacter : (UIWrapper.PlayerCharacter)peer.Player.Character
            };

            UIWrapper.gui_player_state(data);
        }

        public override bool Run()
        {
            try
            {
                if (!UIWrapper.gui_run(_Ready))
                    return false;

                bool running = true;
                while (running)
                {
                    while (UIWrapper.gui_poll_events(out UIWrapper.PollData data))
                    {
                        switch (data.type)
                        {
                            case UIWrapper.PollType.POLL_QUIT:
                                running = false;
                                break;

                            case UIWrapper.PollType.POLL_CLEAR_EXCLUDES:
                                MapVote.Excluded.Clear();
                                MapVote.Excluded.Add(16);
                                break;

                            case UIWrapper.PollType.POLL_ADD_EXCLUDE:
                                MapVote.Excluded.Add(data.value1);
                                break;

                            case UIWrapper.PollType.POLL_KICK:
                                foreach (var server in Program.Servers)
                                {
                                    var session = server.GetSession(data.value1);

                                    if (session == null)
                                        continue;

                                    server.DisconnectWithReason(session, "Kicked by server.");
                                }
                                break;

                            case UIWrapper.PollType.POLL_BAN:
                                foreach (var server in Program.Servers)
                                {
                                    var session = server.GetSession(data.value1);

                                    if (session == null)
                                        continue;

                                    server.DisconnectWithReason(session, "Banned by server.");
                                    if (BanList.Ban(data.value1, out string name, out string ip))
                                        UIWrapper.gui_add_ban(name, ip);

                                }
                                break;

                            case UIWrapper.PollType.POLL_UNBAN:
                                BanList.Unban(data.value2);
                                break;

                            case UIWrapper.PollType.POLL_BACKTOLOBBY:
                                foreach (var server in Program.Servers)
                                {
                                    if (server.State.AsState() != Session.State.LOBBY && server.State.AsState() != Session.State.VOTE)
                                        server.SetState<Lobby>();
                                }
                                break;


                            case UIWrapper.PollType.POLL_EXEWIN:
                                foreach (var server in Program.Servers)
                                {
                                    if(server.State.AsState() == Session.State.GAME)
                                    {
                                        (server.State as Game).EndGame(server, 0);
                                    }
                                }
                                break;


                            case UIWrapper.PollType.POLL_SURVWIN:
                                foreach (var server in Program.Servers)
                                {
                                    if (server.State.AsState() == Session.State.GAME)
                                    {
                                        (server.State as Game).EndGame(server, 1);
                                    }
                                }
                                break;
                        }
                    }

                    Thread.Sleep(10);
                }

                Environment.Exit(0);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private void _Ready()
        {
            try
            {
                Terminal.Log("===================");
                Terminal.Log("TD2DR Server");
                Terminal.Log($"BUILD v{Program.BUILD_VER}");
                Terminal.Log("(c) Team Exe Empire 2023");
                Terminal.Log("===================");
                Terminal.Log("Enter 127.0.0.1 on your PC to join your server.\n");

                for (var i = 0; i < Options.Get<int>("server_count"); i++)
                {
                    Server server = new(i);
                    server.StartAsync();
                    Program.Servers.Add(server);
                }

                foreach (var it in BanList.GetBanned())
                {
                    UIWrapper.gui_add_ban(it.Value, it.Key);
                }
            }
            catch { }
        }
    }
}