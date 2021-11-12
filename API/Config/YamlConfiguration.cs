///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Bukkit'
// https://github.com/Bukkit/Bukkit
// which is released under GNU General Public License v3.0.
// https://www.gnu.org/licenses/gpl-3.0.en.html
// Copyright 2021 Bukkit Team
///////////////////////////////////////////////////////////////////

using API.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace API.Config
{
    /// <summary>
    /// An implementation of <see cref="Configuration" /> which saves all files in Yaml.
    /// </summary>
    /// <remarks> Note that this implementation is not synchronized.</remarks>
    public class YamlConfiguration : Configuration
    {
        public void Save(FileInfo file)
        {
            var serializer = new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            var content = serializer.Serialize(GetValues(false));

            using TextWriter writer = File.CreateText(file.FullName);

            writer.Write(content);
        }

        private void Load(FileSystemInfo file)
        {
            using var reader = File.OpenText(file.FullName);

            var stream = new YamlStream();

            stream.Load(reader);

            foreach (var document in stream.Documents)
            {
                ConvertMapsToSections(document.RootNode, this);
            }
        }

        public void LoadFromString(string contents)
        {
            Validate.NotNull(contents, "Contents cannot be null");

            var stream = new YamlStream();

            stream.Load(new StringReader(contents));

            foreach (var document in stream.Documents)
            {
                ConvertMapsToSections(document.RootNode, this);
            }
        }

        protected void ConvertMapsToSections(YamlNode node, ConfigurationSection section)
        {
            switch (node.NodeType)
            {
                case YamlNodeType.Mapping:
                    var mappingNode = (YamlMappingNode)node;

                    foreach (var (subNode, subValue) in mappingNode.Children)
                    {
                        if (subNode.NodeType == YamlNodeType.Scalar && subValue.AllNodes.Count() == 1)
                        {
                            section.Set(subNode.ToString(), subValue);
                        }
                        else
                        {
                            ConvertMapsToSections(subValue, section.CreateSection(subNode.ToString()));
                        }
                    }

                    break;

                case YamlNodeType.Sequence:
                    var sequenceNode = (YamlSequenceNode)node;

                    var index = 0;

                    foreach (var value in sequenceNode.Children)
                    {
                        section.Set(index.ToString(), value.ToString());
                        index++;
                    }

                    break;

                case YamlNodeType.Alias:
                    throw new Exception("Yaml alias' are not supported.");

                case YamlNodeType.Scalar:
                    throw new Exception("Scalar node at the wrong position.");

                default:
                    throw new ArgumentOutOfRangeException();
            }
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
    }
}