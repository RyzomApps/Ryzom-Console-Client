///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

namespace Client.Network
{
    internal interface IUdpSocket
    {
        /// <summary>
        /// Closes the UDP connection
        /// </summary>
        public void Close();

        /// <summary>
        /// connect to a given frontend address containing a host and a port in the string
        /// </summary>
        public void Connect(string frontendAddress);

        /// <summary>
        /// checks if the udp client is connected
        /// </summary>
        public bool IsConnected();

        /// <summary>
        /// checks if data is available at the client
        /// </summary>
        public bool IsDataAvailable();

        /// <summary>
        /// receives available data
        /// </summary>
        public bool Receive(ref byte[] receiveBuffer, bool throwException);

        /// <summary>
        /// send buffer data to the server
        /// </summary>
        public void Send(byte[] buffer, in int length);
    }
}