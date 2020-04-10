using System;
using System.Net;
using System.Net.Sockets;
using Sockets.EventArgs;

namespace ManagedSockets {
    public class ServerSocket {
        private readonly TcpListener _listener;
        public bool Listening { get; set; }
        private bool _stopping;

        #region Constructors
        /// <summary>
        /// Creates a new ServerSocket to listen on any IP.
        /// </summary>
        /// <param name="port">The port to listen on.</param>
        public ServerSocket(int port) {
            _listener = new TcpListener(IPAddress.Any, port);
        }

        /// <summary>
        /// Creates a serversocket to listen on a predefined ip.
        /// </summary>
        /// <param name="endPoint">The IP/CIDR to listen on</param>
        /// <param name="port">The port  to listen on.</param>
        public ServerSocket(IPAddress endPoint, int port) {
            _listener = new TcpListener(endPoint, port);
        }
        #endregion

        #region public Methods
        /// <summary>
        /// Start listening for connections
        /// </summary>
        public void Listen() {
            if (_listener == null)
                throw new Exception("Class not initialized.");

            Listening = true;
            _listener.Start();
            WaitForClient();
        }

        /// <summary>
        /// Stops listening for new clients.
        /// </summary>
        public void Stop() {
            if (_stopping)
                return;

            _stopping = true;
            _listener.Stop();
            Listening = false;
            ServerStopped?.SafeRaise(null);
        }

        #endregion

        #region Private methods
        /// <summary>
        /// Tries to accept a new client.
        /// </summary>
        private void WaitForClient() {
            try {
                _listener.BeginAcceptTcpClient(AcceptCallback, null);
            }
            catch {
                Stop();
            }
        }
        #endregion

        /// <summary>
        /// Called when a client has connected.
        /// </summary>
        /// <param name="ar"></param>
        private void AcceptCallback(IAsyncResult ar) {
            TcpClient newClient = _listener.EndAcceptTcpClient(ar);
            IncomingClient?.SafeRaise(new IncomingEventArgs(newClient));

            if (Listening && !_stopping)
                WaitForClient(); // -- Continue waiting for a new client.
        }

        #region Events
        /// <summary>
        /// Triggers when a client is connecting to the server
        /// </summary>
        public event IncomingClientEventArgs IncomingClient;
        /// <summary>
        /// Called when the server stops listening.
        /// </summary>
        public event NoEventArgs ServerStopped;

        #endregion
    }
}