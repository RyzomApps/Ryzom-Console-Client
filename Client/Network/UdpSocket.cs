///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

using System;
using System.Net;
using System.Net.Sockets;

namespace Client.Network
{
    /// <summary>
    /// wrapper for the udpclient class to have a synch connection the the server
    /// </summary>
    internal class UdpSocket : IUdpSocket
    {
        private UdpClient _udpMain;

        /// <inheritdoc />
        public void Connect(string frontendAddress)
        {
            ParseHostString(frontendAddress, out var host, out var port);

            _udpMain = new UdpClient();
            _udpMain.Connect(host, port);
        }

        /// <inheritdoc />
        public bool IsConnected()
        {
            return _udpMain.Client.Connected;
        }

        /// <inheritdoc />
        public bool IsDataAvailable()
        {
            return _udpMain.Client?.Available > 0;
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
        public bool Receive(ref byte[] receiveBuffer, bool throwException)
        {
            var remoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);

            try
            {
                var bytes = _udpMain.Receive(ref remoteIpEndPoint);
                Array.Reverse(bytes, 0, bytes.Length);
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
        public void Send(byte[] buffer, in int length)
        {
            _udpMain.Send(buffer, length);
        }

        /// <inheritdoc />
        public void Close()
        {
            _udpMain.Close();
        }
    }
}