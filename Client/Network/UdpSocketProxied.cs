///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

// Ignore Spelling: Proxied

using Client.Network.Proxy;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace Client.Network
{
    /// <summary>
    /// wrapper for the udp client class to have a synchron connection the server
    /// </summary>
    public class UdpSocketProxied : IUdpSocket
    {
        internal const int Timeout = 30000;

        private UdpClient _udpMain;
        private Socket _socks5Socket;
        private IPAddress _ip;
        private ushort _port;
        private byte[] _socksUdpHeader = Array.Empty<byte>();
        private readonly string _proxyAddress;

        private DateTime _lastDataReceived = DateTime.MinValue;

        /// <summary>
        /// Host address without portnumber
        /// </summary>
        public string HostName
        {
            get
            {
                UdpSocket.ParseHostString(_proxyAddress, out var hostName, out _);
                return hostName;
            }
        }


        /// <summary>
        /// Constructor
        /// </summary>
        public UdpSocketProxied(string proxyAddress)
        {
            _proxyAddress = proxyAddress;
        }

        /// <inheritdoc />
        public void Connect(string frontendAddress)
        {
            // Proxy Address
            UdpSocket.ParseHostString(_proxyAddress, out var proxyHost, out var proxyPort);

            var proxyIp = Dns.GetHostAddresses(proxyHost)[0];

            // Ryzom Address
            UdpSocket.ParseHostString(frontendAddress, out var host, out var port);

            _ip = Dns.GetHostAddresses(host)[0];
            _port = (ushort)port;

            // Try to establish a socks5 proxy connection and get port and IP for udp
            _socks5Socket = Socks5Proxy.EstablishConnection(proxyIp.ToString(), (ushort)proxyPort, _ip.ToString(), _port, "anonymous", "", out var udpAddress, out var udpPort);

            // use the proxy host if no other address is given
            if (udpAddress == "\0" || udpAddress == "0.0.0.0")
                udpAddress = proxyHost;

            // connect the udp client to the proxy
            _udpMain = new UdpClient { DontFragment = true, Client = { ReceiveTimeout = Timeout, SendTimeout = Timeout, ReceiveBufferSize = Constants.ReceiveBuffer, DontFragment = true } };

            _udpMain.Connect(udpAddress, udpPort);
        }

        /// <inheritdoc />
        public bool IsConnected()
        {
            return _udpMain.Client.Connected && _socks5Socket.Connected;
        }

        /// <inheritdoc />
        public void Send(byte[] buffer, in int length)
        {
            if (_socksUdpHeader.Length != 10)
            {
                // init the header
                _socksUdpHeader = new byte[10];

                // Type of IP V4 address - TODO: allow usage of IPV6
                _socksUdpHeader[3] = 1;

                // IP
                Array.Copy(_ip.GetAddressBytes(), 0, _socksUdpHeader, 4, 4);

                // Port
                var portBytes = BitConverter.GetBytes(_port);

                Array.Copy(portBytes, 1, _socksUdpHeader, 8, 1);
                Array.Copy(portBytes, 0, _socksUdpHeader, 9, 1);
            }

            var combination = Combine(_socksUdpHeader, buffer);

            _udpMain.Send(combination, _socksUdpHeader.Length + length);
        }

        /// <inheritdoc />
        public bool IsDataAvailable()
        {
            var ret = _udpMain.Client?.Available > 0;

            if (_lastDataReceived == DateTime.MinValue || (DateTime.Now - _lastDataReceived).TotalMilliseconds <= Timeout)
                return ret;

            if (_udpMain.Client?.Connected == true)
                _udpMain.Client?.Disconnect(false);

            if (_socks5Socket.Connected)
                _socks5Socket.Disconnect(false);

            throw new TimeoutException("SOCKS5 Proxy Connection Timeout.");

        }

        /// <inheritdoc />
        public bool Receive(ref byte[] receiveBuffer, bool throwException)
        {
            var remoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);

            try
            {
                var bytes = _udpMain.Receive(ref remoteIpEndPoint);

                // drop any datagram whose FRAG field is other than X'00'
                if (bytes[2] != 0)
                    throw new NotImplementedException("Fragmentation of UDP datagrams is not implemented.");

                // ATYP address type 
                switch (bytes[3])
                {
                    case 0x01:
                        // IP V4 address
                        break;

                    case 0x03:
                        // DOMAINNAME
                        throw new NotImplementedException("DOMAINNAME");

                    case 0x04:
                        // IP V6 address
                        throw new NotImplementedException("IP V6 address");

                    default:
                        throw new NotImplementedException($"ATYP address type {bytes[3]} not implemented.");
                }

                // remove the header - IP V4 only
                bytes = bytes[10..];

                Array.Reverse(bytes, 0, bytes.Length);
                receiveBuffer = bytes;

                if (receiveBuffer.Length > 0)
                    _lastDataReceived = DateTime.Now;
            }
            catch
            {
                if (throwException)
                {
                    throw;
                }

                return false;
            }

            return true;
        }

        /// <inheritdoc />
        public void Close()
        {
            _udpMain.Close();
            _socks5Socket.Close();
        }

        /// <summary>
        /// Function to combine two byte arrays
        /// </summary>
        private static byte[] Combine(byte[] first, byte[] second)
        {
            var bytes = new byte[first.Length + second.Length];
            Buffer.BlockCopy(first, 0, bytes, 0, first.Length);
            Buffer.BlockCopy(second, 0, bytes, first.Length, second.Length);
            return bytes;
        }
    }
}