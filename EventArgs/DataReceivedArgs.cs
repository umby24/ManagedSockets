namespace ManagedSockets.EventArgs {
    public class DataReceivedArgs : ClientEvent {
        public byte[] Data { get; set; }

        public DataReceivedArgs(ClientSocket socket, byte[] data) {
            BaseSocket = socket;
            Data = data;
        }
    }

    public delegate void DataReceivedEventArgs(DataReceivedArgs args);
}
