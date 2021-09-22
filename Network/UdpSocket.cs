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

namespace RCC.Network
{
    /// <summary>
    ///     wrapper for the udpclient class to have a synch connection the the server
    /// </summary>
    internal class UdpSocket
    {
        private UdpClient _udpMain;

        /// <summary>
        ///     connect to a given frontend address containing a host and a port in the string
        /// </summary>
        public void Connect(string frontendAddress)
        {
            ParseHostString(frontendAddress, out var host, out var port);

            _udpMain = new UdpClient();
            _udpMain.Connect(host, port);
        }

        /// <summary>
        ///     get host and port from a address string
        /// </summary>
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

        /// <summary>
        ///     checks if the udp client is connected
        /// </summary>
        public bool Connected()
        {
            return _udpMain.Client.Connected;
        }

        /// <summary>
        ///     send buffer data to the server
        /// </summary>
        public void Send(byte[] buffer, in int length)
        {
            _udpMain.Send(buffer, length);
        }

        /// <summary>
        ///     checks if data is available at the client
        /// </summary>
        public bool IsDataAvailable()
        {
            return _udpMain.Client?.Available > 0;
        }

        /// <summary>
        ///     receives available data
        /// </summary>
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

        /// <summary>
        /// Closes the UDP connection
        /// </summary>
        public void Close()
        {
            _udpMain.Close();
        }
    }
}