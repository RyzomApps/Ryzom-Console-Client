using System.IO;
using RCC.Logger;

namespace RCC.Plugins
{
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
        /// Gets a <see cref="FileConfiguration"/> for this plugin, read through
        /// "config.yml"
        /// <br/>
        /// If there is a default config.yml embedded in this plugin, it will be
        /// provided as a default for this Configuration.
        /// </summary>
        /// <returns>Plugin configuration</returns>
        public FileConfiguration GetConfig();

        /// <summary>
        /// Gets an embedded resource in this plugin
        /// </summary>
        /// <param name="filename">Filename of the resource</param>  
        /// <returns>File if found, otherwise null</returns>
        public Stream GetResource(string filename);

        /// <summary>
        /// Saves the <see cref="FileConfiguration"/> retrievable by <see cref="GetConfig()"/>.
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
        /// <param name="resourcePath">the embedded resource path to look for within the plugin's .jar file. (No preceding slash).</param>  
        /// <param name="replace">if true, the embedded resource will overwrite the contents of an existing file.</param>  
        /// <remarks>throws IllegalArgumentException if the resource path is null, empty,or points to a nonexistent resource.</remarks>
        public void SaveResource(string resourcePath, bool replace);

        /// <summary>
        /// Discards any data in <see cref="GetConfig()"/> and reloads from disk.
        /// </summary>
        public void ReloadConfig();

        /// <summary>
        /// Gets the associated PluginLoader responsible for this plugin
        /// </summary>
        /// <returns>PluginLoader that controls this plugin</returns>
        public PluginLoader GetPluginLoader();

        /// <summary>
        /// Returns the Server instance currently running this plugin
        /// </summary>
        /// <returns>Server running this plugin</returns>
        public RyzomClient GetServer();

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
    }
}
