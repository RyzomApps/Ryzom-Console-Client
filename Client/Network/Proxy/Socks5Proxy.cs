///////////////////////////////////////////////////////////////////
// This file contains modified code from 'L2 .NET'
// https://github.com/devmvalvm/L2Net
// which is released under GNU General Public License v2.0.
// http://www.gnu.org/licenses/
// Copyright 2018 devmvalvm
///////////////////////////////////////////////////////////////////

using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Client.Network.Proxy
{
    /// <summary>
    /// Provides SOCKS5 UDP associate functionality to clients.
    /// </summary>
    /// <remarks>Socks 5 description is available at <a href="http://www.faqs.org/rfcs/rfc1928.html">RFC 1928</a>.</remarks>
    /// <date>23 Jan 2004</date>
    /// <author>zahmed</author>
    /// <date>2022</date>
    /// <author>bierdosenhalter</author>
    public class Socks5Proxy
    {
        private static readonly string[] ErrorMsgs = {
            "Operation completed successfully.",   // 0x00
            "General SOCKS server failure.",       // 0x01
            "Connection not allowed by rule set.", // 0x02
            "Network unreachable.",                // 0x03
            "Host unreachable.",                   // 0x04
            "Connection refused.",                 // 0x05
            "TTL expired.",                        // 0x06
            "Command not supported.",              // 0x07
            "Address type not supported.",         // 0x08
            "Unknown error."                       // 0x09
        };

        private const int ConnectionTimeout = 500;   // 0.5 s for connection phase
        private const int NegotiationTimeout = 5000; //   5 s for negotiation phase
        public const int OperationTimeout = 30000;  //  30 s for normal connection

        /// <summary>
        /// Open a TCP connection to the appropriate SOCKS5 port on the SOCKS5 server system using the UDP associate command
        /// </summary>
        /// <returns>TCP socket</returns>
        public static Socket EstablishConnection(string proxyAddress, ushort proxyPort, string destAddress, ushort destPort, string userName, string password, out string udpAdress, out ushort udpPort)
        {
            var destIP = IPAddress.Parse(destAddress);
            var proxyIP = IPAddress.Parse(proxyAddress);

            byte[] rawBytes;

            var proxyEndPoint = new IPEndPoint(proxyIP, proxyPort);

            // open a TCP connection to SOCKS5 server
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
            {
                // short timeouts to filter proxies with bad pings
                ReceiveTimeout = ConnectionTimeout,
                SendTimeout = ConnectionTimeout,
            };

            var result = socket.BeginConnect(proxyEndPoint, null, null);

            result.AsyncWaitHandle.WaitOne(ConnectionTimeout, true);

            if (socket.Connected)
            {
                socket.EndConnect(result);
            }
            else
            {
                socket.Close();
                throw new ConnectionException("Could not connect to the proxy server.");
            }

            // set timeouts higher for the negotiation phase
            socket.ReceiveTimeout = NegotiationTimeout;
            socket.SendTimeout = NegotiationTimeout;

            // request
            var request = new byte[256];
            ushort nIndex = 0;

            // version 5
            request[nIndex++] = 0x05;

            // 2 authentication methods are in packet
            request[nIndex++] = 0x02;

            // no authentication required
            request[nIndex++] = 0x00;

            // user name and password
            request[nIndex++] = 0x02;

            // send the authentication negotiation request
            socket.Send(request, 0, nIndex, SocketFlags.None);

            // 2 byte response
            var response = new byte[256];
            var nGot = socket.Receive(response, 0, 2, SocketFlags.None);

            // check response length
            if (nGot != 2)
            {
                socket.Close();
                throw new ConnectionException("An incorrect authentication negotiation response was received from the proxy server.");
            }

            // check version
            if (response[0] != 0x05)
            {
                socket.Close();
                throw new ConnectionException($"The proxy server version (0x{response[0]:X2}) is not supported by the client.");
            }

            // authentication
            switch (response[1])
            {
                // none
                case 0x00:
                    break;
                    
                // user name / password
                case 0x02:
                    request = new byte[256];
                    nIndex = 0;
                    request[nIndex++] = 0x01; // Version 5.

                    // user name
                    request[nIndex++] = (byte)userName.Length;
                    rawBytes = Encoding.Default.GetBytes(userName);
                    rawBytes.CopyTo(request, nIndex);
                    nIndex += (ushort)rawBytes.Length;

                    // password
                    request[nIndex++] = (byte)password.Length;
                    rawBytes = Encoding.Default.GetBytes(password);
                    rawBytes.CopyTo(request, nIndex);
                    nIndex += (ushort)rawBytes.Length;

                    // send the user name / password request
                    socket.Send(request, 0, nIndex, SocketFlags.None);

                    // 2 byte response
                    response = new byte[256];
                    nGot = socket.Receive(response, 0, 2, SocketFlags.None);

                    if (nGot != 2)
                    {
                        socket.Close();
                        throw new ConnectionException("An incorrect authentication response was received from the proxy server.");
                    }

                    if (response[1] != 0x00)
                    {
                        socket.Close();
                        throw new ConnectionException("Incorrect user name or password.");
                    }

                    break;

                // no authentication method was accepted close the socket.
                case 0xFF:
                    socket.Close();
                    throw new ConnectionException("The requested authentication method was not accepted by the proxy server.");

                // different method than requested
                default:
                    // 0x01: GSSAPI
                    // 0x03–0x7F: methods assigned by IANA
                    // 0x80–0xFE: methods reserved for private use
                    socket.Close();
                    throw new ConnectionException($"The proxy server is using an authentication method (0x{response[1]:X2}) that is not supported by the client.");
            }

            // send connect request with udp associate command
            nIndex = 0;
            request = new byte[256];

            // version 5.
            request[nIndex++] = 0x05;

            // UDP associate command
            request[nIndex++] = 0x03;

            // reserved
            request[nIndex++] = 0x00;

            // address in IPV4 format
            request[nIndex++] = 0x01;

            // port number
            rawBytes = destIP.GetAddressBytes();
            rawBytes.CopyTo(request, nIndex);
            nIndex += (ushort)rawBytes.Length;

            // using big-endian byte order
            var portBytes = BitConverter.GetBytes(destPort);

            for (var i = portBytes.Length - 1; i >= 0; i--)
                request[nIndex++] = portBytes[i];

            // send connect request
            socket.Send(request, nIndex, SocketFlags.None);

            // get server response
            // protocol version: X'05'
            var ver = new byte[1];
            socket.Receive(ver, 0, 1, SocketFlags.None);

            // reply status
            var rep = new byte[1];
            socket.Receive(rep, 0, 1, SocketFlags.None);

            if (rep[0] != 0x00)
            {
                socket.Close();
                throw new ConnectionException($"Proxy server UDP association failed: {ErrorMsgs[rep[0]]}");
            }

            // reserved
            var rsv = new byte[1];
            socket.Receive(rsv, 0, 1, SocketFlags.None);

            // address type of following address
            var atyp = new byte[1];
            socket.Receive(atyp, 0, 1, SocketFlags.None);

            // server bound address
            switch (atyp[0])
            {
                case 1:
                    // IP V4 address
                    var ip = new byte[4];
                    socket.Receive(ip, 0, 4, SocketFlags.None);
                    udpAdress = new IPAddress(ip).ToString();
                    break;

                case 3:
                    // domain name
                    var len = new byte[1];
                    socket.Receive(len, 0, 1, SocketFlags.None);

                    var domain = new byte[len[0]];
                    socket.Receive(domain, 0, len[0], SocketFlags.None);
                    udpAdress = Encoding.UTF8.GetString(domain);
                    break;

                case 4:
                    // IP V6 address
                    var ip6 = new byte[16];
                    socket.Receive(ip6, 0, 16, SocketFlags.None);
                    udpAdress = new IPAddress(ip6).ToString();
                    break;

                default:
                    socket.Close();
                    throw new ConnectionException($"Proxy server is using an address format (0x{atyp[0]:X2}) that is not supported by the client.");
            }

            // server bound port in network octet order
            var port = new byte[2];
            socket.Receive(port, 0, 2, SocketFlags.None);

            udpPort = BitConverter.ToUInt16(new[] { port[1], port[0] });

            // set timeouts higher for the normal connection
            socket.ReceiveTimeout = OperationTimeout;
            socket.SendTimeout = OperationTimeout;

            // return the socket after the successful connection
            return socket;
        }
    }
}