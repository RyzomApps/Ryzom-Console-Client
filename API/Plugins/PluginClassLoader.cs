using System;
using System.Collections.Generic;
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
    /// TODO: Maybe remove the ClassLoader (since we dont need shared classes) and add the functionality to the PluginLoader
    public class PluginClassLoader : IClassLoader
    {
        private readonly IPluginLoader _pluginLoader;
        private readonly PluginDescriptionFile _description;
        private readonly DirectoryInfo _dataFolder;
        private readonly FileInfo _file;

        public object Class { get; set; }
        private Plugin _pluginInit;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="pluginLoader">plugin loader</param>
        /// <param name="description">plugin.yml information container</param>
        /// <param name="dataFolder">path to the data folder</param>
        /// <param name="file">path to the plugin file</param>
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
                Class = Activator.CreateInstance(type) as Plugin;
            else
                throw new InvalidPluginException($"main class `{description.GetMain()}' does not extend Plugin");

            // Initialize it directly since there are no other class loaders used
            Initialize((Plugin)Class);
        }

        /// <summary>
        /// Initialize a plugin
        /// </summary>
        /// <param name="plugin">Plugin to initialize</param>
        public void Initialize(Plugin plugin)
        {
            Validate.NotNull(plugin, "Initializing plugin cannot be null");

            if (_pluginInit != null)
            {
                throw new ArgumentException("Plugin already initialized!", new Exception("Initial initialization"));
            }

            _pluginInit = plugin;

            plugin.Init(_pluginLoader, _pluginLoader.Handler, _description, _dataFolder, _file, this);
        }

        /// <summary>
        /// Get the plugin class
        /// </summary>
        /// <returns>plugin class</returns>
        public List<string> GetClasses()
        {
            return new List<string> { Class.ToString() };
        }
    }
}