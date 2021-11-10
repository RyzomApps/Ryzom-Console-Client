///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Bukkit'
// https://github.com/Bukkit/Bukkit
// which is released under GNU General Public License v3.0.
// https://www.gnu.org/licenses/gpl-3.0.en.html
// Copyright 2021 Bukkit Team
///////////////////////////////////////////////////////////////////

using System.IO;
using API.Commands;
using API.Logger;
using API.Plugins.Interfaces;

namespace API.Plugins
{
    /// <summary>
    /// Represents a plugin
    /// </summary>
    public abstract class Plugin : IPlugin
    {
        //public PluginClassLoader ClassLoader { get; set; }

        private bool _isEnabled;
        private IPluginLoader _loader;
        private IClient _server;
        private FileInfo _file;
        private PluginDescriptionFile _description;
        private DirectoryInfo _dataFolder;
        private readonly FileConfiguration _newConfig = null;
        private FileInfo _configFile;
        private PluginLoggerWrapper _logger;


        // ReSharper disable once UnusedMember.Global
        protected Plugin() { }

        protected Plugin(IPluginLoader loader, PluginDescriptionFile description, DirectoryInfo dataFolder,
            FileInfo file)
        {
            Init(loader, loader.Handler, description, dataFolder, file, logger);
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
        public IClient GetServer()
        {
            return _server;
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
        public FileConfiguration GetConfig()
        {
            if (_newConfig == null)
            {
                ReloadConfig();
            }
            return _newConfig;
        }

        /// <inheritdoc />
        public void ReloadConfig()
        {
            // TODO: Implement plugin reload config
            throw new System.NotImplementedException();
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
        public void SaveResource(string resourcePath, bool replace)
        {
            // TODO: Save files from plugin resources
            throw new System.NotImplementedException();
        }

        /// <inheritdoc />
        public Stream GetResource(string filename)
        {
            // TODO: Get plugin resources
            throw new System.NotImplementedException();
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

        public void Init(IPluginLoader loader, IClient server, PluginDescriptionFile description, DirectoryInfo dataFolder, FileInfo file)
        {
            _loader = loader;
            _server = server;
            _file = file;
            _description = description;
            _dataFolder = dataFolder;
            //this.classLoader = classLoader;
            _configFile = new FileInfo(dataFolder + "config.yml");
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
