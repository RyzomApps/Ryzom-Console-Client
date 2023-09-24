﻿using API.Logger;
using Client.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;

namespace Client.Network.Proxy
{
    public static class ProxyManager
    {
        internal static UdpSocketProxied workingSocks5Proxy = null;
        internal static string workingSocks5ProxyHostName = "";

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
        internal static UdpSocketProxied GetSocks5Proxy(ILogger logger, string address)
        {
            List<Thread> currentThreads = new List<Thread>();
            workingSocks5Proxy = null;

            // Download and open the proxies file to read from.
            var proxies = DownloadProxyList(logger).Where(hostString => (hostString.Trim().Length != 0 && !hostString.Trim().StartsWith("#") && IPAddress.TryParse(hostString.Split(':')[0], out _))).ToList();
            proxies.Shuffle();

            //var rnd = new Random(DateTime.Now.Millisecond);
            var index = 0;

            // try connections to the proxies until one is working
            while (workingSocks5Proxy == null && index < proxies.Count)
            {
                // Remove old threads
                currentThreads.RemoveAll(t => !t.IsAlive);

                // Start new threads
                if (currentThreads.Count < 32)
                {
                    var hostString = proxies[index];

                    logger?.Info($"[{index}] Trying to use SOCKS5 proxy server '{hostString}'.");

                    var t = new Thread(() => TrySocks5Proxy(logger, address, hostString));
                    currentThreads.Add(t);
                    t.Start();
                    index++;
                    Thread.Sleep(1);
                }
            }

            foreach (var thread in currentThreads)
            {
                //thread.Abort();
            }

            if (workingSocks5Proxy != null)
            {
                return workingSocks5Proxy;
            }

            throw new WebException("Could not find a socks 5 proxy.");
        }

        public static void TrySocks5Proxy(ILogger logger, string address, string hostString)
        {
            try
            {
                var proxy = new UdpSocketProxied(hostString);
                proxy.Connect(address);

                if (workingSocks5Proxy == null)
                {
                    workingSocks5Proxy = proxy;
                }
            }
            catch (Exception innerE)
            {
                if (workingSocks5Proxy == null)
                    logger?.Warn(innerE.Message);
            }
        }

        /// <summary>
        /// Downloads all proxy lists, saves them into a file and returns the list.
        /// </summary>
        /// <returns>string array of proxy server addresses</returns>
        private static string[] DownloadProxyList(ILogger logger)
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
