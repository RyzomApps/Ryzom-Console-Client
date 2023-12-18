using System;
using System.Net;
using Client;
using Client.Network;
using Xunit;
using Client.Network.Proxy;

namespace Tests
{
    public class ProxyTest
    {
        //[Fact]
        //public void Socks5Test()
        //{
        //    Socks5Proxy.EstablishConnection("79.143.187.168", 7497, "164.132.202.87", 53, "", "", out var udpAdress, out var udpPort);
        //
        //    Assert.NotNull(udpAdress);
        //
        //    Assert.Equal("\0", udpAdress);
        //
        //    Assert.Equal(7497, udpPort);
        //}

        [Fact]
        public void DownloadAndTestProxies()
        {
            var client = new RyzomClient(false);

            // Download and open the proxies file to read from.
            var proxies = NetworkConnection.DownloadProxyList(client.GetLogger());

            var rnd = new Random(DateTime.Now.Millisecond);
            var working = false;
            var retryCounter = 1;

            // try connections to the proxies until one is working
            while (!working && retryCounter < 1000)
            {
                var index = rnd.Next(proxies.Length);
                var proxy = proxies[index];

                if (proxy.Trim().Length == 0 || proxy.Trim().StartsWith("#") || !IPAddress.TryParse(proxy.Split(':')[0], out _))
                    continue;

                client.GetLogger().Info($"[{retryCounter}] Trying to use SOCKS5 proxy server '{proxy}'.");

                try
                {
                    var connection = new UdpSocketProxied(proxy);
                    connection.Connect("arma.ryzom.com:47851");
                    working = true;
                }
                catch (Exception innerE)
                {
                    client.GetLogger().Warn(innerE.Message);
                }

                retryCounter++;
            }

            Assert.True(working);
        }
    }
}
