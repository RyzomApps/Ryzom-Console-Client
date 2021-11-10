///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Bukkit'
// https://github.com/Bukkit/Bukkit
// which is released under GNU General Public License v3.0.
// https://www.gnu.org/licenses/gpl-3.0.en.html
// Copyright 2021 Bukkit Team
///////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using API.Exceptions;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace API.Plugins
{
    /// <summary>
    /// This type is the runtime-container for the information in the plugin.yml.
    /// All plugins must have a respective plugin.yml. For plugins using the
    /// standard plugin loader, this file must be in the root of the library as
    /// an embedded resource.
    /// </summary>
    public class PluginDescriptionFile
    {
        [YamlIgnore]
        public string RawName { get; set; }

        public string Name { get; set; }

        public string Main { get; set; }

        public List<string> Depend { get; set; } = new List<string>();

        public List<string> SoftDepend { get; set; } = new List<string>();

        public List<string> LoadBefore { get; set; } = new List<string>();

        public string Version { get; set; }

        public Dictionary<string, Dictionary<string, string>> Commands { get; } = null;

        public string Description { get; set; }

        public List<string> Authors { get; } = null;

        public string Website { get; set; }

        public string Prefix { get; set; }

        /// <summary>
        /// Saves this PluginDescriptionFile to the given writer
        /// </summary>
        public void Save()
        {
            var serializer = new SerializerBuilder()
                .WithNamingConvention(LowerCaseNamingConvention.Instance)
                .Build();

            var yaml = serializer.Serialize(this);

            Console.WriteLine(yaml);
        }

        /// <summary>
        /// Factory for loading a PluginDescriptionFile from the specified file content
        /// </summary>
        /// <param name="content">Text file content</param>
        public static PluginDescriptionFile Load(string content)
        {
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(LowerCaseNamingConvention.Instance)
                .IgnoreUnmatchedProperties()
                .Build();

            var ret = deserializer.Deserialize<PluginDescriptionFile>(content);

            ret.CheckParameters();

            return ret;
        }

        // ReSharper disable once EmptyConstructor
        public PluginDescriptionFile() { }

        /// <summary>
        /// Gives the name of the plugin. This name is a unique identifier for plugins.
        /// </summary>
        /// <returns>the name of the plugin</returns>
        public string GetName()
        {
            return Name.Replace(' ', '_');
        }

        /// <summary>
        /// Gives the version of the plugin.
        /// </summary>
        /// <returns>the version of the plugin</returns>
        public string GetVersion()
        {
            return Version;
        }

        /// <summary>
        /// Gives the fully qualified name of the main class for a plugin. The
        /// format should follow the <see cref="PluginClassLoader"/> syntax
        /// to successfully be resolved at runtime. For most plugins, this is the
        /// class that extends <see cref="Plugin"/>.
        /// </summary>
        /// <returns>the fully qualified main class for the plugin</returns>
        public string GetMain()
        {
            return Main;
        }

        /// <summary>
        /// Gives a human-friendly description of the functionality the plugin provides.
        /// </summary>
        /// <returns>description of this plugin, or null if not specified</returns>
        public string GetDescription()
        {
            return Description;
        }

        /// <summary>
        /// Gives the list of authors for the plugin.
        /// </summary>
        /// <returns>an immutable list of the plugin's authors</returns>
        public List<string> GetAuthors()
        {
            return Authors;
        }

        /// <summary>
        /// Gives the plugin's or plugin's author's website.
        /// </summary>
        /// <returns>description of this plugin, or empty if not specified</returns>
        public string GetWebsite()
        {
            return Website;
        }

        /// <summary>
        /// Gives a list of other plugins that the plugin requires.
        /// </summary>
        /// <returns>immutable list of the plugin's dependencies</returns>
        public List<string> GetDepend()
        {
            return Depend;
        }

        /// <summary>
        /// Gives a list of other plugins that the plugin requires for full
        /// functionality. The <see cref="Interfaces.IPluginManager"/> will make
        /// best effort to treat all entries here as if they were a <see cref="GetDepend()"/>,
        /// but will never fail because of one of these entries.
        /// </summary>
        /// <returns>immutable list of the plugin's preferred dependencies</returns>
        public List<string> GetSoftDepend()
        {
            return SoftDepend;
        }

        /// <summary>
        /// Gets the list of plugins that should consider this plugin a soft-dependency.
        /// </summary>
        /// <returns></returns>
        public List<string> GetLoadBefore()
        {
            return LoadBefore;
        }

        /// <summary>
        /// Gives the token to prefix plugin-specific logging messages with.
        /// </summary>
        /// <returns> immutable list of plugins that should consider this plugin a soft-dependency</returns>
        public string GetPrefix()
        {
            return Prefix;
        }

        /// <summary>
        /// Returns the name of a plugin, including the version. This method is
        /// provided for convenience; it uses the <see cref="GetName()"/> and
        /// <see cref="GetVersion()"/> entries.
        /// </summary>
        /// <returns>a descriptive name of the plugin and respective version</returns>
        public string GetFullName()
        {
            return Name + " v" + Version;
        }

        [Obsolete("Internal use")]
        public string GetRawName()
        {
            return Name;
        }

        /// <summary>
        /// functionality of loadMap of the original bukkit class
        /// </summary>
        private void CheckParameters()
        {
            try
            {
                if (string.IsNullOrEmpty(Name))
                    throw new InvalidDescriptionException("name is not defined");

                RawName = Name;

                if (!new Regex("^[A-Za-z0-9 _.-]+$").IsMatch(Name ?? throw new InvalidOperationException()))
                {
                    throw new InvalidDescriptionException("name \'" + Name + "\' contains invalid characters.");
                }

                Name = Name.Replace(' ', '_');
            }
            catch (Exception ex)
            {
                throw new InvalidDescriptionException(ex, "name is of wrong type");
            }

            if (string.IsNullOrEmpty(Version))
                throw new InvalidDescriptionException("version is not defined");

            if (string.IsNullOrEmpty(Main))
                throw new InvalidDescriptionException("main is not defined");

            Depend = MakePluginNameList(Depend);
            SoftDepend = MakePluginNameList(SoftDepend);
            LoadBefore = MakePluginNameList(LoadBefore);
        }

        /// <summary>
        /// Checks and corrects plugin name lists
        /// </summary>
        private static List<string> MakePluginNameList(List<string> value)
        {
            if (value == null)
            {
                value = new List<string>();
                return value;
            }

            for (var index = 0; index < value.Count; index++)
            {
                value[index] = value[index].Replace(' ', '_');
            }

            return value;
        }
    }
}