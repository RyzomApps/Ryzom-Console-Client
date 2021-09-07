using RCC.Logger;
using System;
using System.Net.Sockets;

namespace RCC
{
    /// <summary>
    /// The main client class, used to connect to a Ryzom server.
    /// </summary>
    public class RyzomClient
    {
        public ILogger Log;

        UdpClient udpClient;

        private int _userAddr;
        private int _userKey;
        private int _userId;
        private int _valid;

        private ConnectionState _connectionState = ConnectionState.NotInitialised;

        public string Cookie { get; set; }

        public string FsAddr { get; set; }

        public string RingMainURL { get; set; }

        public string FarTpUrlBase { get; set; }

        public bool StartStat { get; set; }

        public string R2ServerVersion { get; set; }

        public string R2BackupPatchURL { get; set; }

        public string[] R2PatchUrLs { get; set; }

        /// <summary>
        /// Login to the server using the RyzomCom class.
        /// </summary>
        public void Connect()
        {
            SetLoginCookieFromString(Cookie);

            _connectionState = ConnectionState.NotConnected;

            ConsoleIO.WriteLineFormatted($"Network initialisation with front end '{FsAddr}' and cookie {Cookie}");

            //// connect the session browser to the new shard
            //NLNET::CInetAddress sbsAddress(CSessionBrowserImpl::getInstance().getFrontEndAddress());
            //sbsAddress.setPort(sbsAddress.port() + SBSPortOffset);
            //CSessionBrowserImpl::getInstance().connectItf(sbsAddress);
            //
            //string result;

            ParseHostString(FsAddr, out var host, out var port);

            // then connect to the frontend using the udp sock
            try
            {
                udpClient = new UdpClient();
                udpClient.Connect(host, port);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException($"FS refused the connection ({e.Message})");
            }

            _connectionState = ConnectionState.Login;

            ConsoleIO.WriteLineFormatted(
                $"The next step will be triggered by the CONNECTION:USER_CHARS msg from the server");

            // TODO: Implementation

            ////IPEndPoint object will allow us to read datagrams sent from any source.
            //var remoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
            //
            //// Blocks until a message returns on this socket from a remote host.
            //var receiveBytes = udpClient.Receive(ref remoteIpEndPoint);
            //var returnData = Encoding.ASCII.GetString(receiveBytes);
            //
            //// Uses the IPEndPoint object to determine which of these two hosts responded.
            //Console.WriteLine($"This is the message you received {returnData}");
            //Console.WriteLine($"This message was sent from {remoteIpEndPoint.Address} on their port number {remoteIpEndPoint.Port}");
        }

        /// <summary>
        /// Sets the cookie
        /// </summary>
        /// <param name="str"></param>
        void SetLoginCookieFromString(string str)
        {
            var parts = str.Split('|');

            _userAddr = int.Parse(parts[0], System.Globalization.NumberStyles.HexNumber);
            _userKey = int.Parse(parts[1], System.Globalization.NumberStyles.HexNumber);
            _userId = int.Parse(parts[2], System.Globalization.NumberStyles.HexNumber);

            if (_userAddr == 0 && _userKey == 0 && _userId == 0)
                _valid = 0;
            else
                _valid = 1;
        }

        /// <summary>
        /// Disconnect the client from the server (initiated from MCC)
        /// </summary>
        public void Disconnect()
        {

        }

        public static void ParseHostString(string hostString, out string hostName, out int port)
        {
            hostName = hostString;
            port = -1;

            if (!hostString.Contains(":")) return;

            var hostParts = hostString.Split(':');

            if (hostParts.Length != 2) return;

            hostName = hostParts[0];
            int.TryParse(hostParts[1], out port);
        }
    }

    /// The states of the connection to the server (if you change them, change ConnectionStateCStr)
    internal enum ConnectionState
    {
        NotInitialised = 0,     // nothing happened yet
        NotConnected,           // init() called
        Authenticate,           // connect() called, identified by the login server
        Login,                  // connecting to the frontend, sending identification
        Synchronize,            // connection accepted by the frontend, synchronizing
        Connected,              // synchronized, connected, ready to work
        Probe,                  // connection lost by frontend, probing for response
        Stalled,                // server is stalled
        Disconnect,             // disconnect() called, or timeout, or connection closed by frontend
        Quit                    // quit() called
    };
}
