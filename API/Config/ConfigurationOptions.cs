///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Bukkit'
// https://github.com/Bukkit/Bukkit
// which is released under GNU General Public License v3.0.
// https://www.gnu.org/licenses/gpl-3.0.en.html
// Copyright 2021 Bukkit Team
///////////////////////////////////////////////////////////////////

namespace API.Config
{
    public class ConfigurationOptions
    {
        private char _pathSeparator = '.';
        private bool _copyDefaults;

        private readonly Configuration _configuration;

        public ConfigurationOptions(Configuration configuration)
        {
            _configuration = configuration;
        }

        public Configuration Configuration()
        {
            return _configuration;
        }

        public char PathSeparator()
        {
            return _pathSeparator;
        }

        public ConfigurationOptions PathSeparator(char value)
        {
            _pathSeparator = value;
            return this;
        }

        public bool CopyDefaults()
        {
            return _copyDefaults;
        }

        public ConfigurationOptions CopyDefaults(bool value)
        {
            _copyDefaults = value;
            return this;
        }
    }
}