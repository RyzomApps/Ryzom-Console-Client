﻿///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Bukkit'
// https://github.com/Bukkit/Bukkit
// which is released under GNU General Public License v3.0.
// https://www.gnu.org/licenses/gpl-3.0.en.html
// Copyright 2021 Bukkit Team
///////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.IO;
using API.Config;
using API.Logger;

namespace API.Plugins.Interfaces
{
    /// <summary>
    /// Represents a Plugin<br/>
    /// The use of <see cref="Plugin"/> is recommended for actual Implementation
    /// </summary>
    public interface IPlugin : ICommandExecuter
    {
        /// <summary>
        /// Returns the folder that the plugin data's files are located in. The
        /// folder may not yet exist.
        /// </summary>
        /// <returns>The folder</returns>
        public DirectoryInfo GetDataFolder();

        /// <summary>
        /// Returns the plugin.yaml file containing the details for this plugin
        /// </summary>
        /// <returns>Contents of the plugin.yaml file</returns>
        public PluginDescriptionFile GetDescription();

        /// <summary>
        /// Gets a <see cref="YamlConfiguration"/> for this plugin, read through
        /// "config.yml"
        /// <br/>
        /// If there is a default config.yml embedded in this plugin, it will be
        /// provided as a default for this Configuration.
        /// </summary>
        /// <returns>Plugin configuration</returns>
        public YamlConfiguration GetConfig();

        /// <summary>
        /// Gets an embedded resource in this plugin
        /// </summary>
        /// <param name="filename">Filename of the resource</param>  
        /// <returns>File if found, otherwise null</returns>
        public Stream GetResource(string filename);

        /// <summary>
        /// Saves the <see cref="YamlConfiguration"/> retrievable by <see cref="GetConfig()"/>.
        /// </summary>
        public void SaveConfig();

        /// <summary>
        /// Saves the raw contents of the default config.yml file to the location
        /// retrievable by <see cref="GetConfig()"/>. If there is no default config.yml
        /// embedded in the plugin, an empty config.yml file is saved. This should
        /// fail silently if the config.yml already exists.
        /// </summary>
        public void SaveDefaultConfig();

        /// <summary>
        /// Saves the raw contents of any resource embedded with a plugin's .jar
        /// file assuming it can be found using <see cref="GetResource(string)"/>.
        /// <br/>
        /// The resource is saved into the plugin's data folder using the same
        /// hierarchy as the .jar file (subdirectories are preserved).
        /// </summary>
        /// <param name="fileName">the embedded resource path to look for within the plugin's .jar file. (No preceding slash).</param>  
        /// <param name="replace">if true, the embedded resource will overwrite the contents of an existing file.</param>  
        /// <remarks>throws IllegalArgumentException if the resource path is null, empty,or points to a nonexistent resource.</remarks>
        public void SaveResource(string fileName, bool replace);

        /// <summary>
        /// Discards any data in <see cref="GetConfig()"/> and reloads from disk.
        /// </summary>
        public void ReloadConfig();

        /// <summary>
        /// Gets the associated PluginLoader responsible for this plugin
        /// </summary>
        /// <returns>PluginLoader that controls this plugin</returns>
        public IPluginLoader GetPluginLoader();

        /// <summary>
        /// Returns the Server instance currently running this plugin
        /// </summary>
        /// <returns>Server running this plugin</returns>
        public IClient GetClient();

        /// <summary>
        /// Returns a value indicating whether or not this plugin is currently
        /// enabled
        /// </summary>
        /// <returns>true if this plugin is enabled, otherwise false</returns>
        public bool IsEnabled();

        /// <summary>
        /// Called when this plugin is disabled
        /// </summary>
        public void OnDisable();

        /// <summary>
        /// Called after a plugin is loaded but before it has been enabled.
        /// <br />
        /// When mulitple plugins are loaded, the onLoad() for all plugins is
        /// called before any onEnable() is called.
        /// </summary>
        public void OnLoad();

        /// <summary>
        /// Called when this plugin is enabled
        /// </summary>
        public void OnEnable();

        /// <summary>
        /// Returns the plugin logger associated with this server's logger. The
        /// returned logger automatically tags all log messages with the plugin's
        /// name.
        /// </summary>
        /// <returns>Logger associated with this plugin</returns>
        public ILogger GetLogger();

        /// <summary>
        /// Returns the name of the plugin.
        /// <br/>
        /// This should return the bare name of the plugin and should be used for
        /// comparison.
        /// </summary>
        /// <returns>name of the plugin</returns>
        public string GetName();

        /// <summary>
        /// Perform an internal RCC command (not a server command, use SendText() instead for that!)
        /// </summary>
        /// <param name="command">The command to process</param>
        /// <param name="localVars">Local variables passed along with the command</param>
        /// <returns>TRUE if the command was indeed an internal RCC command</returns>
        public bool PerformInternalCommand(string command, Dictionary<string, object> localVars = null);

        /// <summary>
        /// Perform an internal RCC command (not a server command, use SendText() instead for that!)
        /// </summary>
        /// <param name="command">The command to process</param>
        /// <param name="responseMsg">May contain a confirmation or error message after processing the command, or "" otherwise.</param>
        /// <param name="localVars">Local variables passed along with the command</param>
        /// <returns>TRUE if the command was indeed an internal RCC command</returns>
        public bool PerformInternalCommand(string command, ref string responseMsg, Dictionary<string, object> localVars = null);

        /// <summary>
        /// Register a command in command prompt. Command will be automatically unregistered when unloading the Plugin.
        /// </summary>
        /// <param name="cmdName">Name of the command</param>
        /// <param name="cmdDesc">Description/usage of the command</param>
        /// <param name="cmdUsage">Usage example</param>
        /// <param name="callback">Method for handling the command</param>
        /// <returns>True if successfully registered</returns>
        bool RegisterCommand(string cmdName, string cmdDesc, string cmdUsage, IClient.CommandRunner callback);

        /// <summary>
        /// Will be called every ~100ms.
        /// </summary>
        /// <remarks>
        /// <see cref="ListenerBase.OnUpdate"/> method can be overridden by child class so need an extra update method
        /// </remarks>
        void UpdateInternal();
    }
}
