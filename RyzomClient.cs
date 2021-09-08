using RCC.Logger;
using System;
using System.Diagnostics;
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
        private int _CurrentSendNumber = 0;

        const int SYSTEM_LOGIN_CODE = 0;              // From client
        const int SYSTEM_SYNC_CODE = 1;               // From server
        const int SYSTEM_ACK_SYNC_CODE = 2;           // From client
        const int SYSTEM_PROBE_CODE = 3;              // From server
        const int SYSTEM_ACK_PROBE_CODE = 4;          // From client
        const int SYSTEM_DISCONNECTION_CODE = 5;      // From client
        const int SYSTEM_STALLED_CODE = 6;            // From server
        const int SYSTEM_SERVER_DOWN_CODE = 7;        // From server
        const int SYSTEM_QUIT_CODE = 8;               // From client
        const int SYSTEM_ACK_QUIT_CODE = 9;			  // From server

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

            // stateLogin();
            // sendSystemLogin();

            var message = new CBitMemStream();

            message.BuildSystemHeader(ref _CurrentSendNumber);

            const byte login = SYSTEM_LOGIN_CODE;
            message.Serial(login);

            //message.serial(Cookie);
            message.Serial(_userAddr);
            message.Serial(_userKey);
            message.Serial(_userId);

            // todo: find out why 2 xD
            message.Serial(2);

            message.Serial(ClientCfg.LanguageCode);

            var length = message.Size;

            Debug.WriteLine("00000000 00000000 00000000 00000000 10000000 00000010 11010111 10101110 " +
            "01101100 10001000 11001001 10110110 01010011 00000000 00000101 10011001 " +
            "10011111 00000000 00000000 00000000 00000001 00110010 00110010 1 < P > 0000000");

            foreach (var b in message.Buffer())
            {
                Debug.Write(Convert.ToString(b, 2).PadLeft(8, '0') + " ");
            }

            udpMain.Send(message.Buffer(), length);

            ConsoleIO.WriteLineFormatted(
                $"The next step will be triggered by the CONNECTION:USER_CHARS msg from the server");

            // TODO: Implementation

            // Initializing XML Database
            // IngameDbMngr.init(CPath::lookup("database.xml"), ProgressBar);
        }

        private static void Recv(IAsyncResult ar)
        {
            var remoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
            var client = (UdpClient)ar.AsyncState;

            if (client == null) return;

            var received = client.EndReceive(ar, ref remoteIpEndPoint);

            // TODO: interpret result
            // decode header
            // msgin.serial(_CurrentReceivedNumber);
            // msgin.serial(_SystemMode);


            // msgin.serial(message);

            // SYSTEM_SYNC_CODE=1 or others
            // receiveSystemSync

            // SYSTEM_STALLED_CODE
            // receiveSystemStalled

            // SYSTEM_PROBE_CODE
            // _Changes.push_back(CChange(0, ProbeReceived));
            // receiveSystemProbe

            // SYSTEM_SERVER_DOWN_CODE

            foreach (var b in received)
            {
                Console.Write(Convert.ToString(b, 2).PadLeft(8, '0') + " ");
            }

            //Console.WriteLine(Encoding.UTF8.GetString(received));
        }

        /// <summary>
        /// Sets the cookie
        /// </summary>
        /// <param name="str"></param>
        private void SetLoginCookieFromString(string str)
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
}
