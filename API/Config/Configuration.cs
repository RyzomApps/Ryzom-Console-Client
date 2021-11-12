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
        private Configuration _defaults;

        private ConfigurationOptions _options;

        public Configuration() { }

        public Configuration(Configuration defaults)
        {
            _defaults = defaults;
        }

        public new void AddDefault(string path, object value)
        {
            Validate.NotNull(path, "Path may not be null");

            _defaults ??= new Configuration();

            _defaults.Set(path, value);
        }

        public void AddDefaults(Dictionary<string, object> defaults)
        {
            Validate.NotNull(_defaults, "Defaults may not be null");

            foreach (var (key, value) in defaults)
            {
                base.AddDefault(key, value);
            }

        }

        public void AddDefaults(Configuration defaults)
        {
            Validate.NotNull(defaults, "Defaults may not be null");

            AddDefaults(defaults.GetValues(true));
        }

        public void SetDefaults(Configuration defaults)
        {
            Validate.NotNull(defaults, "Defaults may not be null");

            _defaults = defaults;
        }

        public void SetDefaults(YamlConfiguration defaults)
        {
            Validate.NotNull(defaults, "Defaults may not be null");

            _defaults = defaults;
        }

        public Configuration GetDefaults()
        {
            return _defaults;
        }

        public new ConfigurationSection GetParent()
        {
            return null;
        }

        public ConfigurationOptions Options()
        {
            return _options ??= new ConfigurationOptions(this);
        }
    }
}