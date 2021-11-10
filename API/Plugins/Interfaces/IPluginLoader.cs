///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Bukkit'
// https://github.com/Bukkit/Bukkit
// which is released under GNU General Public License v3.0.
// https://www.gnu.org/licenses/gpl-3.0.en.html
// Copyright 2021 Bukkit Team
///////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.IO;

namespace API.Plugins.Interfaces
{
    /// <summary>
    /// Represents a plugin loader, which handles direct access to specific types
    /// of plugins
    /// </summary>
    public interface IPluginLoader
    {
        /// <inheritdoc cref="IClient"/>
        IClient Handler { get; set; }

        /// <summary>
        /// Loads the plugin contained in the specified file
        /// </summary>
        /// <param name="file">File to attempt to load</param>
        /// <returns>Plugin that was contained in the specified file, or null if unsuccessful</returns>
        /// <throws>InvalidPluginException Thrown when the specified file is not a plugin</throws>
        /// <throws>UnknownDependencyException If a required dependency could not be found</throws>
        IPlugin LoadPlugin(FileInfo file);

        /// <summary>
        /// Loads a PluginDescriptionFile from the specified file
        /// </summary>
        /// <param name="file">File to attempt to load from</param>
        /// <returns>A new PluginDescriptionFile loaded from the plugin.yml in the specified file</returns>
        /// <throws>InvalidDescriptionException If the plugin description file could not be created</throws>
        PluginDescriptionFile GetPluginDescription(FileInfo file);

        /// <summary>
        /// Returns a list of all filename filters expected by this PluginLoader
        /// </summary>
        /// <returns>The filters</returns>
        string[] GetPluginFileFilters();

        /// <summary>
        /// Creates and returns registered listeners for the event classes used in
        /// this listener
        /// </summary>
        /// <param name="listener">The object that will handle the eventual call back</param>
        /// <param name="plugin">The plugin to use when creating registered listeners</param>
        /// <returns>The registered listeners.</returns>
        List<IListener> CreateRegisteredListeners(IListener listener, IPlugin plugin);

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
    }
}