///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Bukkit'
// https://github.com/Bukkit/Bukkit
// which is released under GNU General Public License v3.0.
// https://www.gnu.org/licenses/gpl-3.0.en.html
// Copyright 2021 Bukkit Team
///////////////////////////////////////////////////////////////////

using System;
using System.IO;

namespace API.Plugins.Interfaces
{
    /// <summary>
    /// Handles all plugin management from the Server
    /// </summary>
    public interface IPluginManager
    {
        /// <summary>
        /// Registers the specified plugin loader
        /// </summary>
        /// <param name="loader">Class of the PluginLoader to register</param>
        /// <throws>IllegalArgumentException Thrown when the given Class is not a valid PluginLoader</throws>
        void RegisterInterface(Type loader);

        /// <summary>
        /// Checks if the given plugin is loaded and returns it when applicable <br/>
        /// Please note that the name of the plugin is case-sensitive
        /// </summary>
        /// <param name="name">Name of the plugin to check</param>
        /// <returns>Plugin if it exists, otherwise null</returns>
        IPlugin GetPlugin(string name);

        /// <summary>
        /// Gets a list of all currently loaded plugins
        /// </summary>
        /// <returns>Array of Plugins</returns>
        IPlugin[] GetPlugins();

        /// <summary>
        /// Checks if the given plugin is enabled or not <br/>
        /// Please note that the name of the plugin is case-sensitive.
        /// </summary>
        /// <param name="name">Name of the plugin to check</param>
        /// <returns>true if the plugin is enabled, otherwise false</returns>
        bool IsPluginEnabled(string name);

        /// <summary>
        /// Checks if the given plugin is enabled or not
        /// </summary>
        /// <param name="plugin">Plugin to check</param>
        /// <returns>true if the plugin is enabled, otherwise false</returns>
        bool IsPluginEnabled(IPlugin plugin);

        /// <summary>
        /// Loads the plugin in the specified file <br/>
        /// File must be valid according to the current enabled Plugin interfaces
        /// </summary>
        /// <param name="file">File containing the plugin to load</param>
        /// <returns>The Plugin loaded, or null if it was invalid</returns>
        /// <throws>InvalidPluginException Thrown when the specified file is not a valid plugin</throws>
        /// <throws>InvalidDescriptionException Thrown when the specified file contains an invalid description</throws>
        /// <throws>UnknownDependencyException If a required dependency could not be resolved</throws>
        IPlugin LoadPlugin(FileInfo file);

        /// <summary>
        /// Loads the plugins contained within the specified directory
        /// </summary>
        /// <param name="directory">Directory to check for plugins</param>
        /// <returns>A list of all plugins loaded</returns>
        IPlugin[] LoadPlugins(DirectoryInfo directory);

        /// <summary>
        /// Disables all the loaded plugins
        /// </summary>
        void DisablePlugins();

        /// <summary>
        /// Disables and removes all plugins
        /// </summary>
        void ClearPlugins();

        /// <summary>
        /// Enables the specified plugin <br/>
        /// Attempting to enable a plugin that is already enabled will have no effect
        /// </summary>
        /// <param name="plugin">Plugin to enable</param>
        void EnablePlugin(IPlugin plugin);

        /// <summary>
        /// Disables the specified plugin <br/>
        /// Attempting to disable a plugin that is not enabled will have no effect
        /// </summary>
        /// <param name="plugin">Plugin to disable</param>
        void DisablePlugin(IPlugin plugin);

        /// <summary>
        /// Register a new ListenerBase in this handler list
        /// </summary>
        void RegisterListeners(ListenerBase listenerBase, Plugin plugin, bool init = false);

        /// <summary>
        /// Calls an event with the given details. <br/>
        /// This method only synchronizes when the event is not asynchronous.
        /// </summary>
        /// <param name="action"> Event details</param>
        void CallEvent(Action<ListenerBase> action);
    }
}