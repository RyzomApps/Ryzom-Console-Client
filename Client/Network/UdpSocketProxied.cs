///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

using Client.Network.Proxy;
using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace Client.Network
{
    /// <summary>
    /// wrapper for the udpclient class to have a synch connection the the server
    /// </summary>
    internal class UdpSocketProxied : IUdpSocket
    {
        private UdpClient _udpMain;
        Socket socks5Socket;
        IPAddress _ip;
        ushort _port;
        byte[] socksUdpHeader = new byte[0];
        private string _proxyAddress;

        /// <summary>
        /// Constuctor
        /// </summary>
        public UdpSocketProxied(string proxyAddress)
        {
            _proxyAddress = proxyAddress;
        }

        /// <inheritdoc />
        public void Connect(string frontendAddress)
        {
            // Proxy Address
            ParseHostString(_proxyAddress, out var proxyHost, out var proxyPort);

            // Ryzom Address
            ParseHostString(frontendAddress, out var host, out var port);

            _ip = Dns.GetHostAddresses(host)[0];
            _port = (ushort)port;

            socks5Socket = SocksProxy.ConnectToSocks5Proxy(proxyHost, (ushort)proxyPort, _ip.ToString(), _port, "", "", out string udpAddress, out ushort udpPort);

            if (udpAddress == "\0")
                udpAddress = proxyHost;

            _udpMain = new UdpClient();
            _udpMain.Connect(udpAddress, udpPort);
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
        public bool IsConnected()
        {
            return _udpMain.Client.Connected;
        }

        /// <inheritdoc />
        public void Send(byte[] buffer, in int length)
        {
            if (socksUdpHeader.Length != 10)
            {
                socksUdpHeader = new byte[10];

                // Type of IP V4 address
                socksUdpHeader[3] = 1;

                // IP
                Array.Copy(_ip.GetAddressBytes(), 0, socksUdpHeader, 4, 4);

                // Port
                var portBytes = BitConverter.GetBytes(_port);

                Array.Copy(portBytes, 1, socksUdpHeader, 8, 1);
                Array.Copy(portBytes, 0, socksUdpHeader, 9, 1);
            }

            byte[] combination = Combine(socksUdpHeader, buffer);

            _udpMain.Send(combination, socksUdpHeader.Length + length);
        }

        public static byte[] Combine(byte[] first, byte[] second)
        {
            byte[] bytes = new byte[first.Length + second.Length];
            Buffer.BlockCopy(first, 0, bytes, 0, first.Length);
            Buffer.BlockCopy(second, 0, bytes, first.Length, second.Length);
            return bytes;
        }

        /// <inheritdoc />
        public bool IsDataAvailable()
        {
            return _udpMain.Client?.Available > 0;
        }

        /// <inheritdoc />
        public bool Receive(ref byte[] receiveBuffer, bool throwException)
        {
            var remoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);

            try
            {
                var bytes = _udpMain.Receive(ref remoteIpEndPoint);
                Array.Reverse(bytes, 10, bytes.Length - 10);
                receiveBuffer = bytes;
            }
            catch
            {
                if (throwException)
                    throw;

                return false;
            }

            return true;
        }

        /// <inheritdoc />
        public void Close()
        {
            _udpMain.Close();
            socks5Socket.Close();
        }
    }
}