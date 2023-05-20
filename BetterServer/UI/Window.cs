using BetterServer.Data;

namespace BetterServer.UI
{
    public abstract class Window
    {
        public abstract bool Run();

        public abstract void Log(string message);
        public virtual void AddPlayer(Peer peer) { }
        public virtual void RemovePlayer(Peer peer) { }
    }
}