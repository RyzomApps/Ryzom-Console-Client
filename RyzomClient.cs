using RCC.Logger;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace RCC
{
    /// <summary>
    /// The main client class, used to connect to a Ryzom server.
    /// </summary>
    public class RyzomClient
    {
        public ILogger Log;

        UdpClient udpMain;
        UdpClient udpSessionBrowser;

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

            ParseHostString(FsAddr, out var host, out var port);

            // connect the session browser to the new shard
            try
            {
                udpSessionBrowser = new UdpClient();
                udpSessionBrowser.Connect(host, port + ClientCfg.SBSPortOffset);

                udpSessionBrowser.BeginReceive(Recv, udpSessionBrowser);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException($"SBS refused the connection ({e.Message})");
            }

            // then connect to the frontend using the udp sock
            try
            {
                udpMain = new UdpClient();
                udpMain.Connect(host, port);

                udpMain.BeginReceive(Recv, udpMain);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException($"FS refused the connection ({e.Message})");
            }

            _connectionState = ConnectionState.Login;

            ConsoleIO.WriteLineFormatted(
                $"The next step will be triggered by the CONNECTION:USER_CHARS msg from the server");

            // TODO: Implementation

            // Initializing XML Database
            // IngameDbMngr.init(CPath::lookup("database.xml"), ProgressBar);
        }

        private static void Recv(IAsyncResult ar)
        {
            IPEndPoint remoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
            var client = (UdpClient)ar.AsyncState;

            if (client == null) return;

            var received = client.EndReceive(ar, ref remoteIpEndPoint);
            Console.WriteLine(Encoding.UTF8.GetString(received));
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
