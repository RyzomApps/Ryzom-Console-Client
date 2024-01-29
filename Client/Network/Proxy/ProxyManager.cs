using API.Logger;
using Client.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Client.Network.Proxy
{
    public static class ProxyManager
    {
        internal static UdpSocketProxied workingSocks5ProxyUdp = null;
        internal static Socket workingSocks5ProxyTcp = null;

        private static readonly Random rng = new Random();

        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;

            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        /// TODO: the current method for the threading is unclean and need work -> refactor
        public static UdpSocketProxied GetSocks5ProxyUdp(ILogger logger, string address)
        {
            List<Thread> currentThreads = new List<Thread>();
            workingSocks5ProxyUdp = null;

            // Download and open the proxies file to read from.
            var proxies = DownloadProxyList(logger).Where(hostString => (hostString.Trim().Length != 0 && !hostString.Trim().StartsWith("#") && IPAddress.TryParse(hostString.Split(':')[0], out _))).ToList();
            proxies.Shuffle();

            //var rnd = new Random(DateTime.Now.Millisecond);
            var index = 0;

            // try connections to the proxies until one is working
            while (workingSocks5ProxyUdp == null && index < proxies.Count)
            {
                // Remove old threads
                currentThreads.RemoveAll(t => !t.IsAlive);

                // Start new threads
                if (currentThreads.Count < 32)
                {
                    var hostString = proxies[index];

                    logger?.Info($"[{index}] Trying to use SOCKS5 proxy server '{hostString}'.");

                    var t = new Thread(() => TrySocks5ProxyUdp(logger, address, hostString));
                    currentThreads.Add(t);
                    t.Start();
                    index++;
                    Thread.Sleep(1);
                }
            }

            foreach (var thread in currentThreads)
            {
                try
                {
                    thread.Interrupt();
                    //thread.Join();
                }
                catch { }
            }

            if (workingSocks5ProxyUdp != null)
            {
                return workingSocks5ProxyUdp;
            }

            throw new WebException("Could not find a socks 5 udp proxy.");
        }

        internal static void TrySocks5ProxyUdp(ILogger logger, string address, string hostString)
        {
            try
            {
                var proxy = new UdpSocketProxied(hostString);
                proxy.Connect(address);

                if (workingSocks5ProxyUdp == null)
                {
                    workingSocks5ProxyUdp = proxy;
                }
            }
            catch (Exception innerE)
            {
                if (workingSocks5ProxyUdp == null)
                    logger?.Warn(innerE.Message);
            }
        }

        public static Socket GetSocks5ProxyTcp(ILogger logger, string destAddress)
        {
            List<Thread> currentThreads = new List<Thread>();
            workingSocks5ProxyTcp = null;

            // Download and open the proxies file to read from.
            var proxies = DownloadProxyList(logger).Where(proxy => (proxy.Trim().Length != 0 && !proxy.Trim().StartsWith("#") && IPAddress.TryParse(proxy.Split(':')[0], out _))).ToList();
            proxies.Shuffle();

            //var rnd = new Random(DateTime.Now.Millisecond);
            var index = 0;

            // try connections to the proxies until one is working
            while (workingSocks5ProxyTcp == null && index < proxies.Count)
            {
                // Remove old threads
                currentThreads.RemoveAll(t => !t.IsAlive);

                // Start new threads
                if (currentThreads.Count < 32)
                {
                    var proxyAddress = proxies[index];

                    logger?.Info($"[{index}] Trying to use SOCKS5 proxy server '{proxyAddress}'.");

                    var t = new Thread(() => TrySocks5ProxyTcp(logger, proxyAddress, destAddress));
                    currentThreads.Add(t);
                    t.Start();
                    index++;
                    Thread.Sleep(1);
                }
            }

            foreach (var thread in currentThreads)
            {
                try
                {
                    thread.Interrupt();
                    thread.Join();
                }
                catch { }
            }

            if (workingSocks5ProxyTcp != null)
            {
                return workingSocks5ProxyTcp;
            }

            throw new WebException("Could not find a socks 5 tcp proxy.");
        }

        internal static void TrySocks5ProxyTcp(ILogger logger, string proxyAddress, string destAddress)
        {
            try
            {
                // Proxy Address
                UdpSocket.ParseHostString(proxyAddress, out var proxyHost, out var proxyPort);

                var proxyIp = Dns.GetHostAddresses(proxyHost)[0];

                // Destination Address
                UdpSocket.ParseHostString(destAddress, out var host, out var port);

                var destIp = Dns.GetHostAddresses(host)[0];
                var destPort = (ushort)port;

                // Try to establish a socks5 proxy connection and get port and IP for tcp
                var _socks5Socket = Socks5Proxy.EstablishConnection(proxyIp.ToString(), (ushort)proxyPort, destIp.ToString(), destPort, "anonymous", "", out _, out _, Socks5Proxy.ConnectionRequestCommandType.Connect);

                if (_socks5Socket.Connected == true && workingSocks5ProxyTcp == null)
                {
                    workingSocks5ProxyTcp = _socks5Socket;
                }
            }
            catch (Exception innerE)
            {
                if (workingSocks5ProxyTcp == null)
                    logger?.Warn(innerE.Message);
            }
        }

        /// <summary>
        /// Downloads all proxy lists, saves them into a file and returns the list.
        /// </summary>
        /// <returns>string array of proxy server addresses</returns>
        public static string[] DownloadProxyList(ILogger logger)
        {
            var ret = new List<string>();
#if !DEBUG
            if (!File.Exists("./data/proxies.txt") || (DateTime.Now - File.GetLastWriteTime("./data/proxies.txt")).TotalSeconds > ClientConfig.OnlineProxyListExpiration)
#endif
            foreach (var proxyUrl in ClientConfig.OnlineProxyList)
            {
                try
                {
                    var proxies = new WebClient().DownloadString(proxyUrl).Split('\r', '\n');

                    foreach (var tmpProxy in proxies)
                    {
                        // local copy
                        var proxy = tmpProxy;

                        if (proxy.Contains(" "))
                        {
                            // list with other parameters - assume first row is proxy address
                            proxy = proxy.Split(" ")[0];
                        }

                        // filter invalid list entries
                        if (proxy.Trim().Length == 0 ||
                            proxy.Trim().StartsWith("#") ||
                            !proxy.Contains(":") ||
                            !IPAddress.TryParse(proxy.Split(':')[0], out _))
                            continue;

                        // exclude ephemeral port range (short-lived sessions)
                        if (!int.TryParse(proxy.Split(':')[1], out var port) ||
                            //port >= 1024 && port <= 5000 ||
                            port >= 32768 && port <= 65535)
                            continue;

                        if (!ret.Contains(proxy))
                            ret.Add(proxy);
                    }
                }
                catch
                {
                    logger?.Warn($"Error while processing proxy list from '{proxyUrl}'.");
                }
            }

            if (ret.Count > 0)
            {
                logger?.Info($"Download of proxy server list successful. {ret.Count} proxies total.");
                ret.Sort();
                File.WriteAllLines("./data/proxies.txt", ret, Encoding.UTF8);
            }
            else
            {
                logger?.Info("Local proxy server list has not yet expired or download of the list failed. Using local list.");
                ret = File.ReadAllLines("./data/proxies.txt", Encoding.UTF8).ToList();
            }

            return ret.ToArray();
        }
    }
}
