using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using ManagedSockets.EventArgs;
using Sockets.EventArgs;

namespace ManagedSockets {
    /// <summary>
    /// Client socket
    /// </summary>
    public class ClientSocket {
        /// <summary> True if this socket is connected to a server, false otherwise. </summary>
        public bool IsConnected { get; set; }
        public IPEndPoint Endpoint { get; set; }

        private const int ReadSize = 1024; // -- 1024 keeps memory usage low but may be slow if largers amounts of data are filtering in..
        private const int ConnectTimeout = 15000; // -- 15 seconds

        private TcpClient _baseSocket; // -- The socket used in network communications
        private NetworkStream _baseStream; // -- Network stream from the above socket
        private byte[] _readBuffer = new byte[ReadSize]; // -- Small buffer that async reads will place data into
        private bool _closing, _sending; // -- Internal state flags

        //private ConcurrentQueue<byte[]> _receiveBuffer;
        private ConcurrentQueue<byte[]> _sendBuffer;

        #region Constructors
        /// <summary>
        /// Create a socket ready to connect to the given ip and port.
        /// </summary>
        /// <param name="endpoint">The IP Address/Hostname of the server to connect to</param>
        /// <param name="port">The port to connect to</param>
        public ClientSocket(string endpoint, int port) {
            if (port > 65535 || port < 1) {
                throw new Exception("Port number out of range. Valid range is 1-65535");
            }

            if (endpoint == null) {
                throw new Exception("Given endpoint is null.");
            }

            IPAddress address;
            
            if (!IPAddress.TryParse(endpoint, out address)) {
                // -- Not an IP, try to get it as a dns name..
                IPAddress[] addresses = Dns.GetHostAddresses(endpoint);

                if (addresses == null || addresses.Length == 0) {
                    throw new Exception("Given endpoint is not a valid IP address or hostname.");
                }

                address = addresses[0];
            }
            
            Endpoint = new IPEndPoint(address, port);
            _baseSocket = new TcpClient();
            _sendBuffer = new ConcurrentQueue<byte[]>();
        }

        /// <summary>
        /// Creates a new socket using a given IP Address and port.
        /// </summary>
        /// <param name="address"></param>
        /// <param name="port"></param>
        public ClientSocket(IPAddress address, int port) {
            if (address == null) {
                throw new Exception("Given endpoint is null.");
            }

            if (port > 65535 || port < 1) {
                throw new Exception("Port number out of range. Valid range is 1-65535");
            }

            Endpoint = new IPEndPoint(address, port);
            _baseSocket = new TcpClient();
            _sendBuffer = new ConcurrentQueue<byte[]>();
        }

        /// <summary>
        /// Creates a new socket using a given IP Endpoint.
        /// </summary>
        /// <param name="endpoint"></param>
        public ClientSocket(IPEndPoint endpoint) {
            if (endpoint == null) {
                throw new Exception("Given endpoint is null.");
            }

            Endpoint = endpoint;
            _baseSocket = new TcpClient();
            _sendBuffer = new ConcurrentQueue<byte[]>();
        }

        /// <summary>
        /// Builds a new socket based off an already existing connection
        /// </summary>
        /// <param name="client"></param>
        public ClientSocket(TcpClient client) {
            if (!client.Connected) {
                throw new ArgumentException("Socket is not connected!");
            }

            _baseSocket = client;
            _baseStream = client.GetStream();
            Endpoint = new IPEndPoint(((IPEndPoint)client.Client.RemoteEndPoint).Address, ((IPEndPoint)client.Client.LocalEndPoint).Port);
            IsConnected = true;
            _sendBuffer = new ConcurrentQueue<byte[]>();
            _baseStream.BeginRead(_readBuffer, 0, ReadSize, ReadComplete, null); // -- Begin reading data
        }

        public ClientSocket() {
            _sendBuffer = new ConcurrentQueue<byte[]>();
        }
        #endregion
        #region Public Methods

        public void Accept(TcpClient client) {
            if (!client.Connected) {
                throw new ArgumentException("Socket is not connected!");
            }

            _baseSocket = client;
            _baseStream = client.GetStream();
            Endpoint = new IPEndPoint(((IPEndPoint)client.Client.RemoteEndPoint).Address, ((IPEndPoint)client.Client.LocalEndPoint).Port);
            IsConnected = true;
            _sendBuffer = new ConcurrentQueue<byte[]>();
            try {
                _baseStream.BeginRead(_readBuffer, 0, ReadSize, ReadComplete, null); // -- Begin reading data
            }
            catch {
                Disconnect("Socket Disconnected");
            }
        }
        /// <summary>
        /// Connect to the remote server using the presently set information
        /// </summary>
        public void Connect() {
            if (Endpoint == null) {
                throw new Exception("No connection information provided.");
            }

            if (IsConnected)
                Disconnect("Connect() Called.");

            IAsyncResult handle = _baseSocket.BeginConnect(Endpoint.Address, Endpoint.Port, ConnectComplete, null);

            if (handle.AsyncWaitHandle.WaitOne(ConnectTimeout)) // -- Handle connection timeouts
                return;

            _baseSocket.Close();
            throw new TimeoutException("Failed to connect to the server.");
        }

        /// <summary>
        /// Disconnect from the remote host.
        /// </summary>
        /// <param name="reason">The given reason for the disconnection</param>
        public void Disconnect(string reason) {
            if (!IsConnected || _closing) {
                return;
            }

            _closing = true;
            var counter = 2000;
            //// -- Wait for everything in send buffer to clear??
            while (_sending) {
                //Console.WriteLin("[RAW SOCKET] - WAITING FOR SEND");
                counter -= 1;

                if (counter <= 0)
                    break;

                Thread.Sleep(1);
            }
            
            _sendBuffer = new ConcurrentQueue<byte[]>(); // -- Reset everything
            _readBuffer = new byte[ReadSize];

            _baseStream.Close(); // -- Close our the stream and socket (Forces all async operations to fail over as well)
            _baseSocket.Close();

            IsConnected = false; // -- Flag any users that we are no longer connected.

            // -- Call the disconnected event.
            Disconnected?.SafeRaise(new SocketDisconnectedArgs(this, reason));

            _closing = false; // -- Done closing our connection
        }

        /// <summary>
        /// Send data to the connected server/client
        /// </summary>
        /// <param name="data"></param>
        public async void Send(byte[] data) {
            _sendBuffer.Enqueue(data);

            if (_sending)
                return;

            _sending = true;
            await Task.Run(() => SendLoop());
        }
        #endregion
        #region Async Callbacks


        private void SendLoop() { // -- Potential Bug: Data that needs to be in order, sending in an incorrect order due to async stuff.
            while (_sendBuffer.Count > 0) {
                byte[] data;
                
                if (!_sendBuffer.TryDequeue(out data)) {
                    _sending = false;
                    return;
                }

                try {
                    if (_baseStream.CanWrite) {
                        try {
                            _baseStream.Write(data, 0, data.Length);
                        }
                        catch {
                            Disconnect("Could not send");
                        }
                        //_baseStream.BeginWrite(data, 0, data.Length, DataSent, null);
                    }
                    else {
                        Disconnect("Socket closing");
                    }
                }
                catch {
                    Disconnect("Socket closing");
                }

                Thread.Sleep(1);
            }

            _sending = false;
        }

        private void DataSent(IAsyncResult ar) {
            try {
                _baseStream.EndWrite(ar);
            }
            catch {
                Disconnect("Socket closing");
            }
        }

        private void ConnectComplete(IAsyncResult ar) {
            try {
                _baseSocket.EndConnect(ar); // -- End the connection event..
                IsConnected = true; // -- Flag the system as connected
                _baseStream = _baseSocket.GetStream();
                _baseStream.BeginRead(_readBuffer, 0, ReadSize, ReadComplete, null); // -- Begin reading data

                Connected?.SafeRaise(new SocketConnectedArgs(this)); // -- Trigger the socket connected event.
            }
            catch (Exception ex) {
                IsConnected = false;
                ErrorReceived?.SafeRaise(new SocketErrorArgs(this, "Error during connect: " + ex.Message));
            }
        }

        private void ReadComplete(IAsyncResult ar) {
            int received;

            try {
                received = _baseStream.EndRead(ar); // -- End the async op
            }
            catch (ObjectDisposedException) { // -- Socket closed by client
                return;
            }
            catch (IOException e) {
                if (e.InnerException != null)
                    Disconnect("Socket exception occured: " + e.InnerException.HResult);
                else
                    Disconnect("Socket Exception occured.");

                return;
            }

            if (received == 0) {
                // -- Socket Disconnected.
                Disconnect("Connection closed by remote host.");
                return;
            }

            var newMem = new byte[received];
            Buffer.BlockCopy(_readBuffer, 0, newMem, 0, received); // -- Copy the received data so the end user can use it however they wish
            DataReceived?.SafeRaise(new DataReceivedArgs(this, newMem)); // -- Call the data received event. (Unblocks immediately, async).

            try {
				if (!_closing) 
                    _baseStream.BeginRead(_readBuffer, 0, ReadSize, ReadComplete, null); // -- Read again!
            }
            catch {
                Disconnect("Socket closing");
            }
        }
        #endregion
        #region Events

        public event SocketConnectedEventArgs Connected;
        public event DataReceivedEventArgs DataReceived;
        public event SocketDisconnectedEventArgs Disconnected;
        public event SocketErrorEventArgs ErrorReceived;

        #endregion
    }
}