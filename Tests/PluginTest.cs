using System.IO;
using API.Plugins;
using Client.Logger;
using Xunit;

namespace Tests
{
    public class PluginTest
    {
        [Fact]
        public void PluginInstanceTest()
        {
            var pluginLoader = new PluginLoader();
            var pluginDescriptionFile = new PluginDescriptionFile();
            var dataFolder = new DirectoryInfo("./Plugins/SamplePlugin/");
            var file = new FileInfo("./Plugins/SamplePlugin.dll");
            var logger = new FilteredLogger();

            var plugin = new SamplePlugin.SamplePlugin(pluginLoader, pluginDescriptionFile, dataFolder, file, logger);

            Assert.True(plugin != null);
        }
    }
}

