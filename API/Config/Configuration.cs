///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Bukkit'
// https://github.com/Bukkit/Bukkit
// which is released under GNU General Public License v3.0.
// https://www.gnu.org/licenses/gpl-3.0.en.html
// Copyright 2021 Bukkit Team
///////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using API.Helper;

namespace API.Config
{
    /// <summary>
    /// This is a base class for all File based source of configurable options and settings
    /// </summary>
    public class Configuration : ConfigurationSection
    {
        /// <summary>
        /// Configuration source for default values, or null if none exist.
        /// </summary>
        private Configuration _defaults;

        /// <summary>
        /// Options for this configuration
        /// </summary>
        private ConfigurationOptions _options;

        /// <summary>
        /// Constructor
        /// </summary>
        public Configuration() { }

        /// <summary>
        /// Constructor
        /// </summary>
        public Configuration(Configuration defaults)
        {
            _defaults = defaults;
        }

        /// <summary>
        /// Sets the default value of the given path as provided.
        /// </summary>
        /// <param name="path">Path of the value to set.</param>
        /// <param name="value">Value to set the default to.</param>
        public new void AddDefault(string path, object value)
        {
            Validate.NotNull(path, "Path may not be null");

            _defaults ??= new Configuration();

            _defaults.Set(path, value);
        }

        /// <summary>
        /// Sets the default values of the given paths as provided.
        /// </summary>
        /// <param name="defaults">A map of Path->Values to add to defaults.</param>
        public void AddDefaults(Dictionary<string, object> defaults)
        {
            Validate.NotNull(_defaults, "Defaults may not be null");

            foreach (var (key, value) in defaults)
            {
                base.AddDefault(key, value);
            }

        }

        /// <summary>
        /// Sets the default values of the given paths as provided.
        /// </summary>
        /// <param name="defaults">A configuration holding a list of defaults to copy.</param>
        public void AddDefaults(Configuration defaults)
        {
            Validate.NotNull(defaults, "Defaults may not be null");

            AddDefaults(defaults.GetValues(true));
        }

        /// <summary>
        /// Sets the source of all default values for this <see cref="Configuration"/>.
        /// </summary>
        /// <param name="defaults">New source of default values for this configuration.</param>
        public void SetDefaults(Configuration defaults)
        {
            Validate.NotNull(defaults, "Defaults may not be null");

            _defaults = defaults;
        }

        /// <summary>
        /// Gets the source <see cref="Configuration"/> for this configuration.
        /// </summary>
        public Configuration GetDefaults()
        {
            return _defaults;
        }

        /// <summary>
        /// Gets the <see cref="ConfigurationOptions"/> for this <see cref="Configuration"/>.
        /// </summary>
        /// <returns>Options for this configuration</returns>
        public ConfigurationOptions Options()
        {
            return _options ??= new ConfigurationOptions(this);
        }
    }
}