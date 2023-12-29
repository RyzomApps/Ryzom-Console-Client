using System;
using System.Net;
using Client;
using Client.Network;
using Xunit;
using Client.Network.Proxy;
using System.Net.Sockets;
using System.Text;

namespace Tests
{
    public class ProxyTest
    {
        //[Fact]
        //public void TestUdpProxy()
        //{
        //    var client = new RyzomClient(false);
        //
        //    // Download and open the proxies file to read from.
        //    var proxies = ProxyManager.DownloadProxyList(client.GetLogger());
        //
        //    var rnd = new Random(DateTime.Now.Millisecond);
        //    var working = false;
        //    var retryCounter = 1;
        //
        //    // try connections to the proxies until one is working
        //    while (!working && retryCounter < 100)
        //    {
        //        var index = rnd.Next(proxies.Length);
        //        var proxy = proxies[index];
        //
        //        if (proxy.Trim().Length == 0 || proxy.Trim().StartsWith("#") || !IPAddress.TryParse(proxy.Split(':')[0], out _))
        //            continue;
        //
        //        client.GetLogger().Info($"[{retryCounter}] Trying to use SOCKS5 proxy server '{proxy}'.");
        //
        //        try
        //        {
        //            var connection = new UdpSocketProxied(proxy);
        //            connection.Connect("arma.ryzom.com:47851");
        //            working = true;
        //        }
        //        catch (Exception innerE)
        //        {
        //            client.GetLogger().Warn(innerE.Message);
        //        }
        //
        //        retryCounter++;
        //    }
        //
        //    Assert.True(working);
        //}

        [Fact]
        public void TestTcpProxy()
        {
            var client = new RyzomClient(false);

            var _socks5Socket = ProxyManager.GetSocks5ProxyTcp(client.GetLogger(), "arma.ryzom.com:80");

            var requestStr = "GET https://arma.ryzom.com HTTP/1.1\r\n" +
            "Host: arma.ryzom.com\r\n" +
            "User-Agent: Ryzom/Omega / v23.12.346 #adddfe118-windows-x64\r\n" +
            "Accept: */*\r\n" +
            "Accept-Language: en\r\n" +
            "Accept-Charset: utf-8\r\n" +
            //"Accept-Encoding: gzip, deflate\r\n" +
            //"Connection: close\r\n" +
            "\r\n";

            byte[] remoteRequest = Encoding.UTF8.GetBytes(requestStr);
            _socks5Socket.Send(remoteRequest);

            int count;
            string remoteStr = "";
            byte[] remoteBuffer = new byte[1024];

            while ((count = _socks5Socket.Receive(remoteBuffer)) != 0)
            {
                string receivedData = Encoding.UTF8.GetString(remoteBuffer, 0, count);
                remoteStr += receivedData;
            }

            client.GetLogger().Info(remoteStr);

            Assert.Contains("301 Moved Permanently", remoteStr);
        }
    }
}
