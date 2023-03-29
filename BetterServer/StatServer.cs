using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace BetterServer
{
    /// <summary>
    /// Show statistics on connection
    /// </summary>
    //public class StatServer : TcpServer
    //{
    //    public StatServer() : base(IPAddress.Any, 3444)
    //    {
    //        OptionSendBufferSize = 2 + Program.Config.ServerCount * 2;
    //    }
    //
    //    protected override void OnStarted()
    //    {
    //        Logger.Log("Status server is listening on 3444");
    //        base.OnStarted();
    //    }
    //
    //    protected override void OnConnected(TcpSession session)
    //    {
    //        using (MemoryStream str = new())
    //        {
    //            using (BinaryWriter wr = new(str))
    //            {
    //                wr.Write((byte)Program.Config.ServerCount);
    //                wr.Write((byte)Program.MAX_PLAYERS);
    //
    //                foreach (var server in Program.Servers)
    //                {
    //                    wr.Write((byte)server.State.AsState());
    //                    wr.Write((byte)server.Peers.Count);
    //                }
    //            }
    //            session.SendAsync(str.ToArray());
    //        }
    //
    //        session.Disconnect();
    //        base.OnConnected(session);
    //    }
    //}
}
