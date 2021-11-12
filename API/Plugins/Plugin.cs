///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Bukkit'
// https://github.com/Bukkit/Bukkit
// which is released under GNU General Public License v3.0.
// https://www.gnu.org/licenses/gpl-3.0.en.html
// Copyright 2021 Bukkit Team
///////////////////////////////////////////////////////////////////

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using API.Commands;
using API.Config;
using API.Logger;
using API.Plugins.Interfaces;

namespace API.Plugins
{
    /// <summary>
    /// Represents a plugin
    /// </summary>
    public abstract class Plugin : IPlugin
    {
        private bool _isEnabled;
        private IPluginLoader _loader;
        private IClient _client;
        private FileInfo _file;
        private PluginDescriptionFile _description;
        private DirectoryInfo _dataFolder;
        private PluginClassLoader _classLoader;
        private YamlConfiguration _newConfig;
        private FileInfo _configFile;
        private PluginLoggerWrapper _logger;

        // ReSharper disable once UnusedMember.Global
        protected Plugin() { }

        // ReSharper disable once UnusedMember.Global
        protected Plugin(PluginClassLoader classLoader, IPluginLoader loader, PluginDescriptionFile description, DirectoryInfo dataFolder, FileInfo file)
        {
            Init(loader, loader.Handler, description, dataFolder, file, classLoader);
        }

        /// <inheritdoc />
        public DirectoryInfo GetDataFolder()
        {
            return _dataFolder;
        }

        /// <inheritdoc />
        public IPluginLoader GetPluginLoader()
        {
            return _loader;
        }

        /// <inheritdoc />
        public IClient GetClient()
        {
            return _client;
        }

        /// <inheritdoc />
        public bool IsEnabled()
        {
            return _isEnabled;
        }

        /// <summary>
        /// Returns the file which contains this plugin
        /// </summary>
        /// <returns>File containing this plugin</returns>
        protected FileInfo GetFile()
        {
            return _file;
        }

        /// <inheritdoc />
        public PluginDescriptionFile GetDescription()
        {
            return _description;
        }

        /// <inheritdoc />
        public YamlConfiguration GetConfig()
        {
            if (_newConfig == null)
            {
                ReloadConfig();
            }

            return _newConfig;
        }

        /// <summary>
        /// Iterate all types within the specified assembly.<br/>
        /// Check whether that's the shortest so far.<br/>
        /// If it's, set it to the ns.
        /// </summary>
        /// <param name="asm">Assembly to check</param>
        /// <returns>Return the shortest namespace of the assembly</returns>
        /// TODO: Move to helper class
        public static string GetAssemblyNamespace(Assembly asm)
        {
            var ns = "";

            foreach (var tp in asm.Modules.First().GetTypes())
                if (tp.Namespace != null && (ns.Length == 0 || tp.Namespace.Length < ns.Length))
                    ns = tp.Namespace;

            return ns;
        }

        /// <inheritdoc />
        public void ReloadConfig()
        {
            _newConfig = YamlConfiguration.LoadConfiguration(_configFile, _client);

            var defConfigStream = GetResource("config.yml");

            if (defConfigStream == null)
                return;

            using var reader = new StreamReader(defConfigStream);

            var defConfig = new YamlConfiguration();

            try
            {
                defConfig.LoadFromString(reader.ReadToEnd());
            }
            catch (Exception e)
            {
                _client.GetLogger().Error("Cannot load configuration from jar\r\n" + e);
            }

            _newConfig.SetDefaults(defConfig);
        }

        /// <inheritdoc />
        public void SaveConfig()
        {
            try
            {
                GetConfig().Save(_configFile);
            }
            catch (IOException ex)
            {
                _logger.Error($"Could not save config to {_configFile}\r\n{ex}");
            }
        }

        /// <inheritdoc />
        public void SaveDefaultConfig()
        {
            if (!_configFile.Exists)
            {
                SaveResource("config.yml", false);
            }
        }

        /// <inheritdoc />
        public void SaveResource(string fileName, bool replace)
        {
            if (fileName == null || fileName.Equals(""))
            {
                throw new ArgumentException("ResourcePath cannot be null or empty");
            }

            var inputStream = GetResource(fileName);

            if (inputStream == null)
            {
                throw new ArgumentException($"The embedded resource '{fileName}' cannot be found in {_file}");
            }

            var outFile = new FileInfo($"{_dataFolder}\\{fileName}");

            if (!_dataFolder.Exists)
            {
                Directory.CreateDirectory(_dataFolder.FullName);
            }

            try
            {
                if (!outFile.Exists || replace)
                {
                    using var outputStream = File.Create(outFile.FullName);

                    inputStream.Seek(0, SeekOrigin.Begin);
                    inputStream.CopyTo(outputStream);
                }
                else
                {
                    _client.GetLogger().Error($"Could not save {outFile.Name} to {outFile.DirectoryName} because it already exists.");
                }
            }
            catch (IOException ex)
            {
                _client.GetLogger().Error($"Could not save {outFile.Name} to {outFile}", ex);
            }
        }

        /// <inheritdoc />
        public Stream GetResource(string filename)
        {
            var assembly = Assembly.GetAssembly(GetType());

            if (assembly == null)
                return null;

            var nameSpace = GetType().Namespace;
            var resourceName = $"{nameSpace}.{filename}";

            return assembly.GetManifestResourceStream(resourceName);
        }

        /// <summary>
        /// Returns the ClassLoader which holds this plugin
        /// </summary>
        /// <returns>ClassLoader holding this plugin</returns>
        public PluginClassLoader GetClassLoader()
        {
            return _classLoader;
        }

        /// <summary>
        /// Sets the enabled state of this plugin
        /// </summary>
        /// <param name="enabled">true if enabled, otherwise false</param>  
        public void SetEnabled(bool enabled)
        {
            if (_isEnabled == enabled) return;

            _isEnabled = enabled;

            if (_isEnabled)
            {
                OnEnable();
            }
            else
            {
                OnDisable();
            }
        }

        public void Init(IPluginLoader loader, IClient server, PluginDescriptionFile description, DirectoryInfo dataFolder, FileInfo file, PluginClassLoader classLoader)
        {
            _loader = loader;
            _client = server;
            _file = file;
            _description = description;
            _dataFolder = dataFolder;
            _classLoader = classLoader;
            _configFile = new FileInfo($@"{dataFolder}\config.yml");
            _logger = new PluginLoggerWrapper(this, server.GetLogger());
        }

        /// <inheritdoc />
        public virtual bool OnCommand(object sender, ICommand command, string label, string[] args)
        {
            return false;
        }

        /// <inheritdoc />
        public virtual void OnLoad() { }

        /// <inheritdoc />
        public virtual void OnDisable() { }

        /// <inheritdoc />
        public virtual void OnEnable() { }

        /// <inheritdoc />
        public virtual ILogger GetLogger()
        {
            return _logger;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return GetName().GetHashCode();
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (this == obj)
            {
                return true;
            }
            if (obj == null)
            {
                return false;
            }
            return obj is Plugin plugin && GetName().Equals(plugin.GetName());
        }

        /// <inheritdoc />
        public string GetName()
        {
            return GetDescription().GetName();
        }
    }
}
