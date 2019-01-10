using System;
using System.Net.Sockets;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Sockets.Tests {
    [TestClass]
    public class UnitTest1 {
        [TestMethod]
        [ExpectedException(typeof(Exception), "Should throw invalid port exception.")]
        public void Constructor_InvalidPort() {
            var mySocket = new ClientSocket("127.0.0.1", 99999);
        }

        [TestMethod]
        [ExpectedException(typeof(SocketException), "Should throw invalid IP Exception.")]
        public void Consturctor_InvalidIp() {
            var mySocket = new ClientSocket("932.129.32.428", 25565);
        }

        [TestMethod]
        [ExpectedException(typeof(SocketException), "Should throw invalid host Exception.")]
        public void Constructor_InvalidDnsName() {
            var mySocket = new ClientSocket("oihfihqowhie.net", 25565);
        }

        [TestMethod]
        public void Constructor_ClosedPort() {
            var mySocket = new ClientSocket("127.0.0.1", 65222);
        }

        [TestMethod]
        public void Connect_Valid() {
            var mySocket = new ClientSocket("google.com", 80);
            bool connectEventFired = false;

            mySocket.Connected += args => { connectEventFired = true; };

            mySocket.Connect();

            Thread.Sleep(3000); // -- Wait a moment..

            Assert.IsTrue(mySocket.IsConnected, "Failed to connect to a valid host.");
            Assert.IsTrue(connectEventFired, "The connection event did not fire.");
        }

        [TestMethod]
        [ExpectedException(typeof(TimeoutException))]
        public void Connect_InvalidPort() {
            var mySocket = new ClientSocket("google.com", 802);
            bool connectEventFired = false;

            mySocket.Connected += args => { connectEventFired = true; };

            mySocket.Connect();

            Assert.IsFalse(mySocket.IsConnected, "Managed to connect to invalid port.");
            Assert.IsFalse(connectEventFired, "The connection event fired.");
        }

        [TestMethod]
        [ExpectedException(typeof(TimeoutException))]
        public void Connect_InvalidIp() {
            var mySocket = new ClientSocket("172.16.0.1", 802);
            bool connectEventFired = false;

            mySocket.Connected += args => { connectEventFired = true; };

            mySocket.Connect();

            Assert.IsFalse(mySocket.IsConnected, "Managed to connect to invalid address.");
            Assert.IsFalse(connectEventFired, "The connection event fired.");
        }

        //[TestMethod]
        //public void Connect_InvalidIp_Caught() {
        //    var mySocket = new ClientSocket("127.0.0.1", 802);
        //    bool connectEventFired = false, errorFired = false, disconnectFired = false;

        //    mySocket.Connected += args => { connectEventFired = true; };
        //    mySocket.ErrorReceived += args => { errorFired = true; };
        //    mySocket.Disconnected += args => { disconnectFired = true; };

        //    try {
        //        mySocket.Connect();
        //    }
        //    catch (Exception ex) {
        //        Console.WriteLine("Error " + ex.Message);
        //    }

        //    Assert.IsFalse(mySocket.IsConnected, "Managed to connect to invalid address.");
        //    Assert.IsFalse(connectEventFired, "The connection event fired.");
        //    Assert.IsFalse(disconnectFired, "Disconnect fired.");
        //    Assert.IsTrue(errorFired, "Error failed to fire.");
        //}
    }

}
