using System;
using System.Net;
using System.Net.Sockets;

namespace RCC.Network
{
    class UdpSimSock
    {
        private UdpClient udpMain;

        public void connect(string frontendAddress)
        {
            ParseHostString(frontendAddress, out var host, out var port);

            udpMain = new UdpClient();
            udpMain.Connect(host, port);
        }

        public void ParseHostString(string hostString, out string hostName, out int port)
        {
            hostName = hostString;
            port = -1;

            if (!hostString.Contains(":")) return;

            var hostParts = hostString.Split(':');

            if (hostParts.Length != 2) return;

            hostName = hostParts[0];
            int.TryParse(hostParts[1], out port);
        }

        public bool connected()
        {
            return udpMain.Client.Connected;
        }

        public void send(byte[] buffer, in int length)
        {
            udpMain.Send(buffer, length);
        }

        public bool dataAvailable()
        {
            return udpMain.Client.Available > 0;
        }

        public bool receive(ref byte[] _ReceiveBuffer, int len, bool throw_exception)
        {
            var remoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);

            try
            {
                byte[] bytes = udpMain.Receive(ref remoteIpEndPoint);
                Array.Reverse(bytes, 0, bytes.Length);
                _ReceiveBuffer = bytes;
            }
            catch
            {
                if (throw_exception)
                    throw;

                return false;
            }

            return true;
        }

        public void close()
        {
            udpMain.Close();
        }
    }
}
