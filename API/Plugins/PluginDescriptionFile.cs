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

        public void Save()
        {
            var serializer = new SerializerBuilder().WithNamingConvention(LowerCaseNamingConvention.Instance).Build();

            var yaml = serializer.Serialize(this);

            Console.WriteLine(yaml);
        }

        public static PluginDescriptionFile Load(string content)
        {
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(LowerCaseNamingConvention.Instance) // see height_in_inches in sample yml 
                .IgnoreUnmatchedProperties()
                .Build();

            var ret = deserializer.Deserialize<PluginDescriptionFile>(content);

            ret.CheckParameters();

            return ret;
        }

        // ReSharper disable once EmptyConstructor
        public PluginDescriptionFile() { }

        public string GetName()
        {
            return Name.Replace(' ', '_');
        }

        public string GetVersion()
        {
            return Version;
        }

        public string GetMain()
        {
            return Main;
        }

        public string GetDescription()
        {
            return Description;
        }

        public List<string> GetAuthors()
        {
            return Authors;
        }

        public string GetWebsite()
        {
            return Website;
        }

        public IEnumerable<string> GetDepend()
        {
            return Depend;
        }

        public List<string> GetSoftDepend()
        {
            return SoftDepend;
        }

        public List<string> GetLoadBefore()
        {
            return LoadBefore;
        }

        public string GetPrefix()
        {
            return Prefix;
        }

        public string GetFullName()
        {
            return Name + " v" + Version;
        }

        public string GetRawName()
        {
            return Name;
        }

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