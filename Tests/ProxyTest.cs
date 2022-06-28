using System.Diagnostics;
using Xunit;
using Client.Network.Proxy;
using System.Net.Sockets;

namespace Tests
{
    public class ProxyTest
    {
        [Fact]
        public void Socks5Test()
        {
            var socket = SocksProxy.ConnectToSocks5Proxy("79.143.187.168", 7497, "164.132.202.87", 53, "", "", out string udpAdress, out ushort udpPort);

            Debug.Print(udpAdress + " " + udpPort);
        }
    }
}
