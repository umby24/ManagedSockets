namespace Sockets.EventArgs {
    public abstract class ClientEvent : System.EventArgs {
        public ClientSocket BaseSocket { get; set; }
    }
}
