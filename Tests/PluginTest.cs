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
            var pluginPath = new FileInfo(@"..\plugins\SamplePlugin.dll");

            var client = new RyzomClient(false);

            var pluginLoader = new PluginLoader(client);

            var plugin = pluginLoader.LoadPlugin(pluginPath);

            Assert.True(plugin != null);
        }

        [Fact]
        public void PluginDescriptionTest()
        {
            var pluginPath = new FileInfo(@"..\plugins\SamplePlugin.dll");

            Assert.True(pluginPath.Exists);

            var client = new RyzomClient(false);

            var pluginLoader = new PluginLoader(client);
            var desc = pluginLoader.GetPluginDescription(pluginPath);

            Assert.NotNull(desc);
        }

        [Fact]
        public void YamlTest()
        {
            var pdf = new PluginDescriptionFile();

            pdf.Save();

            Assert.NotNull(pdf);
        }

        [Fact]
        public void YamlConfigurationFileTest()
        {
            var pluginPath = new FileInfo(@"..\plugins\SamplePlugin.dll");

            var client = new RyzomClient(false);

            var pluginLoader = new PluginLoader(client);

            var plugin = pluginLoader.LoadPlugin(pluginPath);

            plugin.SaveDefaultConfig();

            var config = plugin.GetConfig();

            config.AddDefault("test", true);

            Assert.NotNull(config.Get("test"));
        }
    }
}

