using System.IO;
using API.Plugins;
using Client;
using Client.Plugins;
using Xunit;

namespace Tests
{
    public class PluginTest
    {
        [Fact]
        public void PluginLoaderTest()
        {
            var pluginPath = new FileInfo("SamplePlugin.dll");
            
            var client = new RyzomClient(false);

            var pluginLoader = new DotNetPluginLoader(client);

            var plugin = pluginLoader.LoadPlugin(pluginPath);

            Assert.True(plugin != null);
        }

        [Fact]
        public void PluginDescriptionTest()
        {
            var pluginPath = new FileInfo("SamplePlugin.dll");

            Assert.True(pluginPath.Exists);

            var client = new RyzomClient(false);

            var pluginLoader = new DotNetPluginLoader(client);
            var desc = pluginLoader.GetPluginDescription(pluginPath);

            Assert.NotNull(desc);
        }

        [Fact]
        public void YamlTest()
        {
            var pdf = new PluginDescriptionFile("");

            pdf.Save();

            Assert.NotNull(pdf);
        }
    }
}

