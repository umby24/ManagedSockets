using System.Net.Sockets;

namespace Sockets.EventArgs {
    public class IncomingEventArgs : System.EventArgs {
        public TcpClient IncomingClient { get; set; }

        public IncomingEventArgs(TcpClient client) {
            IncomingClient = client;
        }
    }

    public delegate void IncomingClientEventArgs(IncomingEventArgs args);
}