using System.Runtime.InteropServices;

namespace BetterServer.UI
{

    public class UIWrapper
    {
        public enum PollType
        {
            POLL_NONE,
            POLL_QUIT,

            POLL_CLEAR_EXCLUDES,
            POLL_ADD_EXCLUDE,

            POLL_KICK,
            POLL_BAN,
            POLL_UNBAN,
            POLL_BACKTOLOBBY,
            POLL_EXEWIN,
            POLL_SURVWIN
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct PollData
        {
            public ushort value1;
            public PollType type;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string value2;
        }

        public enum PlayerCharacter
        {
            CHARACTER_NONE,

            CHRACTER_TAILS,
            CHARACTER_KNUX,
            CHARACTER_EGGMAN,
            CHARACTER_AMY,
            CHARACTER_CREAM,
            CHARACTER_SALLY,

            CHARACTER_EXE,
            CHARACTER_CHAOS,
            CHARACTER_EXETIOR,
            CHARACTER_EXELLER
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct PlayerData
        {
            public int state;
            public ushort pid;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string name;

            public PlayerCharacter character;
        }

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void ReadyCallback();

#if _WINDOWS
        [DllImport("UI/ServerGUI.dll", CallingConvention = CallingConvention.Winapi, SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool gui_run([MarshalAs(UnmanagedType.FunctionPtr)] ReadyCallback cb);

        [DllImport("UI/ServerGUI.dll", CallingConvention = CallingConvention.Winapi, SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool gui_poll_events(out PollData data);

        [DllImport("UI/ServerGUI.dll", CallingConvention = CallingConvention.Winapi, SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern void gui_log(string text);

        [DllImport("UI/ServerGUI.dll", CallingConvention = CallingConvention.Winapi, SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern void gui_add_ban(string name, string ip);

        [DllImport("UI/ServerGUI.dll", CallingConvention = CallingConvention.Winapi, SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern void gui_set_status(string text);

        [DllImport("UI/ServerGUI.dll", CallingConvention = CallingConvention.Winapi, SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern void gui_player_state(PlayerData data);


        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool AllocConsole();

#else
    [DllImport("UI/libServerGUI.so", CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Unicode)]
    public static extern bool gui_run(ReadyCallback cb);

    [DllImport("UI/libServerGUI.so", CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Unicode)]
    public static extern bool gui_poll_events(out PollData data);
        
    [DllImport("UI/libServerGUI.so", CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Unicode)]
    public static extern void gui_log(string text);

    [DllImport("UI/libServerGUI.so", CallingConvention = CallingConvention.Winapi, SetLastError = true, CharSet = CharSet.Unicode)]
    public static extern void gui_add_ban(string name, string ip);

    [DllImport("UI/libServerGUI.so", CallingConvention = CallingConvention.Winapi, SetLastError = true, CharSet = CharSet.Unicode)]
    public static extern void gui_set_status(string text);

    [DllImport("UI/libServerGUI.so", CallingConvention = CallingConvention.Winapi, SetLastError = true, CharSet = CharSet.Unicode)]
    public static extern void gui_player_state(PlayerData data);
#endif    
    }
}