namespace ManagedSockets.EventArgs {
    public class SocketDisconnectedArgs : ClientEvent {
        public string Reason { get; set; }

        public SocketDisconnectedArgs(ClientSocket socket, string reason) {
            Reason = reason;
            BaseSocket = socket;
        }
    }

    public delegate void SocketDisconnectedEventArgs(SocketDisconnectedArgs args);
}
