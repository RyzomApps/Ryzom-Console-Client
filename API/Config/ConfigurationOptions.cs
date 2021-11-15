///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Bukkit'
// https://github.com/Bukkit/Bukkit
// which is released under GNU General Public License v3.0.
// https://www.gnu.org/licenses/gpl-3.0.en.html
// Copyright 2021 Bukkit Team
///////////////////////////////////////////////////////////////////

namespace API.Config
{
    /// <summary>
    /// Various settings for controlling the input and output of a <see cref="Configuration"/>
    /// </summary>
    public class ConfigurationOptions
    {
        /// <summary>
        /// Path separator
        /// </summary>
        private char _pathSeparator = '.';

        /// <summary>
        /// value Whether or not defaults are directly copied
        /// </summary>
        private bool _copyDefaults;

        /// <summary>
        /// Parent configuration
        /// </summary>
        private readonly Configuration _configuration;

        /// <summary>
        /// Constructor
        /// </summary>
        public ConfigurationOptions(Configuration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Returns the <see cref="Configuration"/> that this object is responsible for.
        /// </summary>
        /// <returns>Parent configuration</returns>
        public Configuration Configuration()
        {
            return _configuration;
        }
        
        /// <summary>
        /// Gets the char that will be used to separate <see cref="ConfigurationSection"/>s<br/>
        /// This value does not affect how the {@link Configuration} is stored, only in how you access the data. The default value is '.'.
        /// </summary>
        /// <returns>Path separator</returns>
        public char PathSeparator()
        {
            return _pathSeparator;
        }

        /// <summary>
        /// Sets the char that will be used to separate <see cref="ConfigurationSection"/>s<br/>
        /// This value does not affect how the {@link Configuration} is stored, only in how you access the data. The default value is '.'.
        /// </summary>
        /// <param name="value">Path separator</param>
        /// <returns>This object, for chaining</returns>
        public ConfigurationOptions PathSeparator(char value)
        {
            _pathSeparator = value;
            return this;
        }

        /// <summary>
        /// Checks if the {@link Configuration} should copy values from its default <see cref="Configuration"/> directly.
        /// </summary>
        /// <returns>Whether or not defaults are directly copied</returns>
        public bool CopyDefaults()
        {
            return _copyDefaults;
        }

        /// <summary>
        /// Sets if the {@link Configuration} should copy values from its default <see cref="Configuration"/> directly.
        /// </summary>
        /// <param name="value">value Whether or not defaults are directly copied</param>
        /// <returns>This object, for chaining</returns>
        public ConfigurationOptions CopyDefaults(bool value)
        {
            _copyDefaults = value;
            return this;
        }
    }
}