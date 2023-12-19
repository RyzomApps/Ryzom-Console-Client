using System;
using System.Diagnostics;
using Client;
using Client.Plugins;
using System.IO;
using Xunit;
using Client.Network.WebIG;

namespace Tests
{
    public class ClientTest
    {
        [Fact]
        public void ClientInstanceTest()
        {
            var client = new RyzomClient(false);

            Assert.True(client != null);
        }

        [Fact]
        public void PluginManagerTest()
        {
            var client = new RyzomClient(false);

            var loader = new PluginManager(client);

            Debug.Print(Environment.CurrentDirectory);

            loader.RegisterInterface(typeof(PluginLoader));

            var plugins = loader.LoadPlugins(new DirectoryInfo(@"../plugins/"));

            Assert.True(plugins.Length > 0);
        }

        [Fact]
        public void BrowserTest()
        {
            var client = new RyzomClient(false);

            var webigThread = new WebigNotificationThread(client);
            webigThread.Init();
            webigThread.Get("http://www.google.de");
        }

        [Fact]
        public void ProxyTest()
        {
            var client = new RyzomClient(false);

            var webigThread = new WebigNotificationThread(client);
            webigThread.Init();

            var proxyThread = new HttpProxyServerThread(client, webigThread);
            proxyThread.Init();
        }
    }
}
