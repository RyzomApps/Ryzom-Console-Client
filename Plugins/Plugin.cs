///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Bukkit'
// https://github.com/Bukkit/Bukkit
// which is released under GNU General Public License v3.0.
// https://www.gnu.org/licenses/gpl-3.0.en.html
// Copyright 2021 Bukkit Team
///////////////////////////////////////////////////////////////////

using RCC.Commands.Internal;
using RCC.Logger;
using System.IO;

namespace RCC.Plugins
{
    /// <summary>
    /// Represents a plugin
    /// </summary>
    public abstract class Plugin : IPlugin
    {
        private bool _isEnabled = false;
        private PluginLoader _loader = null;
        private RyzomClient _server = null;
        private FileInfo _file = null;
        private PluginDescriptionFile _description = null;
        private DirectoryInfo _dataFolder = null;
        private readonly FileConfiguration _newConfig = null;
        private FileInfo _configFile = null;
        private PluginLogger _logger = null;

        protected void JavaPlugin(PluginLoader loader, PluginDescriptionFile description, DirectoryInfo dataFolder, FileInfo file)
        {
            Init(loader, loader.Server, description, dataFolder, file);
        }

        /// <inheritdoc />
        public DirectoryInfo GetDataFolder()
        {
            return _dataFolder;
        }

        /// <inheritdoc />
        public PluginLoader GetPluginLoader()
        {
            return _loader;
        }

        /// <inheritdoc />
        public RyzomClient GetServer()
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
        protected void SetEnabled(bool enabled)
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

        private void Init(PluginLoader loader, RyzomClient server, PluginDescriptionFile description, DirectoryInfo dataFolder, FileInfo file /*, object classLoader*/)
        {
            _loader = loader;
            _server = server;
            _file = file;
            _description = description;
            _dataFolder = dataFolder;
            //this.classLoader = classLoader;
            _configFile = new FileInfo(dataFolder + "config.yml");
            _logger = new PluginLogger(this);
        }

        /// <inheritdoc />
        public bool OnCommand(object sender, CommandBase command, string label, string[] args)
        {
            return false;
        }

        /// <inheritdoc />
        public void OnLoad() { }

        /// <inheritdoc />
        public void OnDisable() { }

        /// <inheritdoc />
        public void OnEnable() { }

        /// <inheritdoc />
        public ILogger GetLogger()
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
