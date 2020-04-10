namespace ManagedSockets.EventArgs {
    public class SocketErrorArgs : ClientEvent {
        public string Error { get; set; }

        public SocketErrorArgs(ClientSocket socket, string message) {
            BaseSocket = socket;
            Error = message;
        }

    }

    public delegate void SocketErrorEventArgs(SocketErrorArgs args);
}
