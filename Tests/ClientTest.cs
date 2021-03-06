using System;
using System.Diagnostics;
using Client;
using Client.Plugins;
using System.IO;
using Xunit;

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
    }
}
