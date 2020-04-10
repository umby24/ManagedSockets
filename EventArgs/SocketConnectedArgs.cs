namespace ManagedSockets.EventArgs {
    public class SocketConnectedArgs : ClientEvent {

        public SocketConnectedArgs(ClientSocket socket) {
            BaseSocket = socket;
        }
    }

    public delegate void SocketConnectedEventArgs(SocketConnectedArgs args);
}
