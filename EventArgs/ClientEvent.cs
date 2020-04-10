namespace ManagedSockets.EventArgs {
    public abstract class ClientEvent : System.EventArgs {
        public ClientSocket BaseSocket { get; set; }
    }
}
