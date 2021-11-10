using System;
using System.IO;
using System.Reflection;
using API.Exceptions;
using API.Helper;
using API.Plugins.Interfaces;

namespace API.Plugins
{
    /// <summary>
    /// A ClassLoader for plugins, to allow shared classes across multiple plugins [bdh: well not rly (yet)]
    /// </summary>
    /// TODO: Maybe remove the PluginClassLoader (since we dont need shared classes) and add the functionality to the PluginLoader
    public class PluginClassLoader
    {
        private readonly IPluginLoader _pluginLoader;
        private readonly PluginDescriptionFile _description;
        private readonly DirectoryInfo _dataFolder;
        private readonly FileInfo _file;

        public Plugin Plugin { get; set; }
        private Plugin _pluginInit;

        public PluginClassLoader(IPluginLoader pluginLoader, PluginDescriptionFile description, DirectoryInfo dataFolder, FileInfo file)
        {
            _pluginLoader = pluginLoader;
            _description = description;
            _dataFolder = dataFolder;
            _file = file;

            var asm = Assembly.LoadFile(file.FullName);

            var type = asm.GetType(description.GetMain());

            if (type == null)
                throw new InvalidPluginException($"Cannot find main class `{description.GetMain()}'");

            if (type.BaseType == typeof(Plugin))
                Plugin = Activator.CreateInstance(type) as Plugin;
            else
                throw new InvalidPluginException($"main class `{description.GetMain()}' does not extend Plugin");

            // Initialize it directly since there are no other class loaders used
            Initialize(Plugin);
        }

        public void Initialize(Plugin javaPlugin)
        {
            Validate.NotNull(javaPlugin, "Initializing plugin cannot be null");

            if (_pluginInit != null)
            {
                throw new ArgumentException("Plugin already initialized!", new Exception("Initial initialization"));
            }

            _pluginInit = javaPlugin;

            javaPlugin.Init(_pluginLoader, _pluginLoader.Handler, _description, _dataFolder, _file);
        }
    }
}