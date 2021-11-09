using System;
using System.IO;
using System.Reflection;
using API.Exceptions;
using API.Helper;
using API.Logger;
using API.Plugins.Interfaces;

namespace API.Plugins
{
    public class PluginClassLoader
    {
        private readonly IPluginLoader _csharpPluginLoader;
        private readonly PluginDescriptionFile _description;
        private readonly DirectoryInfo _dataFolder;
        private readonly FileInfo _file;

        public CsharpPlugin Plugin { get; set; }
        private CsharpPlugin _pluginInit;

        public PluginClassLoader(IPluginLoader csharpPluginLoader, PluginDescriptionFile description, DirectoryInfo dataFolder, FileInfo file)
        {
            _csharpPluginLoader = csharpPluginLoader;
            _description = description;
            _dataFolder = dataFolder;
            _file = file;

            var asm = Assembly.LoadFile(file.FullName);

            var type = asm.GetType(description.GetMain());

            if(type==null)
                throw new InvalidPluginException($"Cannot find main class `{description.GetMain()}'");

            if (type.BaseType == typeof(CsharpPlugin))
                Plugin = Activator.CreateInstance(type) as CsharpPlugin;
            else
                throw new InvalidPluginException($"main class `{description.GetMain()}' does not extend CsharpPlugin");

            Initialize(Plugin);

            //if (Plugin != null) Plugin.ClassLoader = this;
        }

        public void Initialize(CsharpPlugin javaPlugin) {
            Validate.NotNull(javaPlugin, "Initializing plugin cannot be null");
            //Validate.IsTrue(javaPlugin.GetClassLoader() == this, "Cannot initialize plugin outside of this class loader");
            
            if (_pluginInit != null) {
                throw new ArgumentException("Plugin already initialized!", new Exception("Initial initialization"));
            }

            _pluginInit = javaPlugin;

            //var logger = new PluginLoggerWrapper(javaPlugin, _csharpPluginLoader.Handler.GetLogger());

            javaPlugin.Init(_csharpPluginLoader, _csharpPluginLoader.Handler, _description, _dataFolder, _file);
        }
    }
}