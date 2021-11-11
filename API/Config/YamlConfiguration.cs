///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Bukkit'
// https://github.com/Bukkit/Bukkit
// which is released under GNU General Public License v3.0.
// https://www.gnu.org/licenses/gpl-3.0.en.html
// Copyright 2021 Bukkit Team
///////////////////////////////////////////////////////////////////

using System;
using System.IO;
using API.Helper;
using YamlDotNet.RepresentationModel;

namespace API.Config
{
    /// <summary>
    /// This is a base class for all File based source of configurable options and settings
    /// </summary>
    public class YamlConfiguration : Configuration
    {
        private YamlDocument _yaml;

        public void Save(FileInfo file)
        {
            var stream = new YamlStream { _yaml };

            using TextWriter writer = File.CreateText(file.FullName);

            stream.Save(writer, false);
        }

        private void Load(FileSystemInfo file)
        {
            using var reader = File.OpenText(file.FullName);

            var stream = new YamlStream();

            stream.Load(reader);

            _yaml = stream.Documents[0];
        }

        public void LoadFromString(string contents)
        {
            Validate.NotNull(contents, "Contents cannot be null");

            var stream = new YamlStream();

            stream.Load(new StringReader(contents));

            _yaml = stream.Documents[0];
        }

        public static YamlConfiguration LoadConfiguration(FileInfo file, IClient client)
        {
            Validate.NotNull(file, "File cannot be null");

            var config = new YamlConfiguration();

            try
            {
                config.Load(file);
            }
            catch (FileNotFoundException)
            {

            }
            catch (Exception ex)
            {
                client.GetLogger().Error($"Cannot load {file}\r\n{ex}");
            }

            return config;
        }

        public void SetDefaults(YamlConfiguration _)
        {
            //throw new NotImplementedException();
        }

        private bool _copyDefaults;

        public void CopyDefaults(bool value)
        {
            _copyDefaults = value;
        }

        public bool CopyDefaults()
        {
            return _copyDefaults;
        }
    }
}