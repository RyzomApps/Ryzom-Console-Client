using API.Logger;
using Client.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Client.Network.Proxy
{
    public static class ProxyManager
    {
        public static List<string> brokenHosts = [];

        internal static UdpSocketProxied workingSocks5ProxyUdp = null;
        internal static Socket workingSocks5ProxyTcp = null;

        private static readonly Random rng = new Random();

        /// <summary>
        /// Attempts to find a working SOCKS5 UDP proxy server.
        /// </summary>
        /// <param name="logger">Logger for logging information.</param>
        /// <param name="address">The destination address to connect through the proxy.</param>
        /// <returns>A Socket object if a working SOCKS5 UDP proxy is found; otherwise, an exception is thrown.</returns>
        public static UdpSocketProxied GetSocks5ProxyUdp(ILogger logger, string address)
        {
            List<Thread> currentThreads = [];
            workingSocks5ProxyUdp = null;

            // Download and open the proxies file to read from.
            var proxies = DownloadProxyList(RyzomClient.GetInstance().GetLogger()).Where(hostString => (hostString.Trim().Length != 0 && !hostString.Trim().StartsWith("#") && IPAddress.TryParse(hostString.Split(':')[0], out _))).ToList();
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

                    //logger?.Info($"[{index}] Trying to use SOCKS5 proxy server '{hostString}'.");

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

        /// <summary>
        /// Attempts to find a working SOCKS5 TCP proxy server.
        /// </summary>
        /// <param name="logger">Logger for logging information.</param>
        /// <param name="address">The destination address to connect through the proxy.</param>
        /// <returns>A Socket object if a working SOCKS5 TCP proxy is found; otherwise, an exception is thrown.</returns>
        public static Socket GetSocks5ProxyTcp(ILogger logger, string destAddress)
        {
            List<Thread> currentThreads = [];
            workingSocks5ProxyTcp = null;

            // Download and open the proxies file to read from.
            var proxies = DownloadProxyList(RyzomClient.GetInstance().GetLogger()).Where(proxy => proxy.Trim().Length != 0 && !proxy.Trim().StartsWith('#') && IPAddress.TryParse(proxy.Split(':')[0], out _)).ToList();
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

                    //logger?.Info($"[{index}] Trying to use SOCKS5 proxy server '{proxyAddress}'.");

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
                    //thread.Join();
                }
                catch { }
            }

            if (workingSocks5ProxyTcp != null)
            {
                return workingSocks5ProxyTcp;
            }

            throw new WebException("Could not find a socks 5 tcp proxy.");
        }

        /// <summary>
        /// Adds a current proxy to the list of broken hosts.
        /// </summary>
        internal static void SetProxyBrokenFlag(bool udpProxy)
        {
            if (udpProxy && workingSocks5ProxyUdp != null)
                brokenHosts.Add(workingSocks5ProxyUdp.HostName);
            else if (!udpProxy && workingSocks5ProxyTcp != null)
                brokenHosts.Add(workingSocks5ProxyTcp.RemoteEndPoint.ToString());
        }

        /// <summary>
        /// Downloads all proxy lists, saves them into a file and returns the list.
        /// </summary>
        /// <returns>string array of proxy server addresses</returns>
        public static string[] DownloadProxyList(ILogger logger)
        {
            var ret = new List<string>();

            // Check if local file exists and is fresh
            if (!File.Exists("./data/proxies.txt") || (DateTime.Now - File.GetLastWriteTime("./data/proxies.txt")).TotalSeconds > ClientConfig.OnlineProxyListExpiration)
            {
                var proxyUrls = ClientConfig.OnlineProxyList;

                logger?.Info($"Downloading proxy ips from {proxyUrls.Length} proxy lists. This could take some time...");

                var proxiesLock = new object(); // To synchronize access to ret

                // Use Parallel.ForEach for concurrent downloads
                _ = Parallel.ForEach(proxyUrls, (proxyUrl, state, index) =>
                {
                    logger?.Info($"Downloading proxy list from {proxyUrl}...");

                    try
                    {
                        var proxies = new TimedWebClient { Timeout = 5000 }.DownloadString(proxyUrl).Split(["\r", "\n"], StringSplitOptions.None);

                        var localProxies = new List<string>();

                        foreach (var tmpProxy in proxies)
                        {
                            var proxy = tmpProxy;

                            if (proxy.Contains(' '))
                            {
                                proxy = proxy.Split(' ')[0];
                            }

                            var trimmedProxy = proxy.Trim();

                            if (string.IsNullOrEmpty(trimmedProxy) ||
                                trimmedProxy.StartsWith('#') ||
                                !trimmedProxy.Contains(':') ||
                                !IPAddress.TryParse(trimmedProxy.Split(':')[0], out _))
                                continue;

                            // Exclude local addresses
                            if (trimmedProxy.StartsWith("0.0.0.0") || trimmedProxy.StartsWith("127.0.0.1"))
                                continue;

                            localProxies.Add(trimmedProxy);
                        }

                        lock (proxiesLock)
                        {
                            foreach (var p in localProxies)
                            {
                                if (!ret.Contains(p))
                                    ret.Add(p);
                            }
                        }
                    }
                    catch
                    {
                        logger?.Warn($"Error while processing proxy list from '{proxyUrl}'.");
                    }
                });

                logger?.Info($"Download of proxy server list successful. {ret.Count} proxies total.");
            }
            else
            {
                // Load from local cache
                ret = [.. File.ReadAllLines("./data/proxies.txt", Encoding.UTF8)];
            }

            if (ret.Count > 0)
            {
                ret.Sort();
                File.WriteAllLines("./data/proxies.txt", ret, Encoding.UTF8);
            }
            else
            {
                logger?.Info("Local proxy server list has not yet expired or download of the list failed. Using local list.");
                ret = [.. File.ReadAllLines("./data/proxies.txt", Encoding.UTF8)];
            }

            return [.. ret];
        }

        private static void TrySocks5ProxyUdp(ILogger logger, string address, string hostString)
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

        private static void TrySocks5ProxyTcp(ILogger logger, string proxyAddress, string destAddress)
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

        private static void Shuffle<T>(this IList<T> list)
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
    }

    public class TimedWebClient : WebClient
    {
        /// <summary>Timeout in milliseconds, default = 600,000 msec</summary>
        public int Timeout { get; set; }

        public TimedWebClient() => Timeout = 600000;

        protected override WebRequest GetWebRequest(Uri address)
        {
            var objWebRequest = base.GetWebRequest(address);
            objWebRequest.Timeout = this.Timeout;
            return objWebRequest;
        }
    }
}
