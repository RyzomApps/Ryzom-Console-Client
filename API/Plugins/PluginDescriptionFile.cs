///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Bukkit'
// https://github.com/Bukkit/Bukkit
// which is released under GNU General Public License v3.0.
// https://www.gnu.org/licenses/gpl-3.0.en.html
// Copyright 2021 Bukkit Team
///////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text.RegularExpressions;
using API.Exceptions;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace API.Plugins
{
    public class PluginDescriptionFile
    {
        public string RawName { get; private set; }
        public string Name { get; private set; }
        public string Main { get; private set; }

        public List<string> Depend { get; } = new List<string>();
        public List<string> SoftDepend { get; } = new List<string>();
        public List<string> LoadBefore { get; } = new List<string>();

        public string Version { get; private set; }
        public Dictionary<string, Dictionary<string, string>> Commands { get; } = null;
        public string Description { get; private set; }
        public List<string> Authors { get; } = null;
        public string Website { get; private set; }
        public string Prefix { get; private set; }

        //private bool database = false;

        private object order = null; //PluginLoadOrder.POSTWORLD;
        //private List<Permission> permissions = null;
        //private Map<?, ?> lazyPermissions = null;
        //private PermissionDefault defaultPerm = PermissionDefault.OP;
        //private Set<PluginAwareness> awareness = ImmutableSet.of();

        public void Save()
        {
            var serializer = new SerializerBuilder()
                .WithNamingConvention(LowerCaseNamingConvention.Instance)
                .Build();

            var yaml = serializer.Serialize(this);

            Console.WriteLine(yaml);
        }

        public static PluginDescriptionFile Load(string content)
        {
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(LowerCaseNamingConvention.Instance) // see height_in_inches in sample yml 
                .IgnoreUnmatchedProperties()
                .Build();

            return deserializer.Deserialize<PluginDescriptionFile>(content);
        }

        [Obsolete("Just for testing purpose")]
        public PluginDescriptionFile()
        {

        }

        public PluginDescriptionFile(string content)
        {
            //this.content = content;
        }

        public string GetName()
        {
            return Name.Replace(' ', '_');;
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

        public object GetLoad()
        {
            return order;
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

        //public Map<String, Map<String, Object>> getCommands() {
        //    return commands;
        //}

        public string GetFullName()
        {
            return Name + " v" + Version;
        }

        public string GetRawName()
        {
            return Name;
        }

        private void LoadMap(Dictionary<string, object> map)
        {
            try
            {
                RawName = map["name"].ToString();
                Name = map["name"].ToString();

                if (!new Regex("^[A-Za-z0-9 _.-]+$").IsMatch(Name ?? throw new InvalidOperationException()))
                {
                    throw new InvalidDescriptionException(("name \'" + (Name + "\' contains invalid characters.")));
                }

                Name = Name.Replace(' ', '_');
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidDescriptionException(ex, "name is not defined");
            }
            catch (Exception ex)
            {
                throw new InvalidDescriptionException(ex, "name is of wrong type");
            }

            try
            {
                Version = map["version"].ToString();
            }
            catch (Exception ex)
            {
                throw new InvalidDescriptionException(ex, "version is not defined");
            }
            //catch (Exception ex)
            //{
            //    throw new InvalidDescriptionException(ex, "version is of wrong type");
            //}

            try
            {
                Main = map["main"].ToString();

                if (Main.StartsWith("org.bukkit."))
                {
                    throw new InvalidDescriptionException("main may not be within the org.bukkit namespace");
                }

            }
            catch (Exception ex)
            {
                throw new InvalidDescriptionException(ex, "main is not defined");
            }
            //catch (Exception ex)
            //{
            //    throw new InvalidDescriptionException(ex, "main is of wrong type");
            //}

            //if ((map["commands"] != null))
            //{
            //    ImmutableMap.Builder<String, Map<String, Object>> commandsBuilder = ImmutableMap.<;
            //    String;
            //    Map<String, Object> Greater;
            //    builder();
            //    try
            //    {
            //        foreach (Map.Entry command in ((Map)(map["commands"))).entrySet())
            //        {
            //            ImmutableMap.Builder<String, Object> commandBuilder = ImmutableMap.<;
            //            String;
            //            (Object > builder());
            //            if ((command.getValue() != null))
            //            {
            //                foreach (Map.Entry commandEntry in ((Map)(command.getValue())).entrySet())
            //                {
            //                    if ((commandEntry.getValue() is Iterable))
            //                    {
            //                        //  This prevents internal alias list changes
            //                        ImmutableList.Builder<Object> commandSubList = ImmutableList.<;
            //                        (Object > builder());
            //                        foreach (Object commandSubListItem in ((Iterable)(commandEntry.getValue())))
            //                        {
            //                            if ((commandSubListItem != null))
            //                            {
            //                                commandSubList.add(commandSubListItem);
            //                            }
            //
            //                        }
            //
            //                        commandBuilder.put(commandEntry.getKey().ToString(), commandSubList.build());
            //                    }
            //                    else if ((commandEntry.getValue() != null))
            //                    {
            //                        commandBuilder.put(commandEntry.getKey().ToString(), commandEntry.getValue());
            //                    }
            //
            //                }
            //
            //            }
            //
            //            commandsBuilder.put(command.getKey().ToString(), commandBuilder.build());
            //        }
            //
            //    }
            //    catch (Exception ex)
            //    {
            //        throw new InvalidDescriptionException(ex, "commands are of wrong type");
            //    }
            //
            //    commands = commandsBuilder.build();
            //}

            //if ((map["class-loader-of"] != null))
            //{
            //    classLoaderOf = map["class-loader-of"].ToString();
            //}

            //depend = makePluginNameList(map, "depend");
            //softDepend = makePluginNameList(map, "softdepend");
            //loadBefore = makePluginNameList(map, "loadbefore");

            //if ((map["database"] != null))
            //{
            //    try
            //    {
            //        database = ((bool)(map["database"]));
            //    }
            //    catch (Exception ex)
            //    {
            //        throw new InvalidDescriptionException(ex, "database is of wrong type");
            //    }
            //
            //}

            if ((map["website"] != null))
            {
                Website = map["website"].ToString();
            }

            if ((map["description"] != null))
            {
                Description = map["description"].ToString();
            }

            //if ((map["load"] != null))
            //{
            //    try
            //    {
            //        order = PluginLoadOrder.valueOf(((String)(map["load"))).toUpperCase().replaceAll("\\\\W", ""));
            //    }
            //    catch (Exception ex)
            //    {
            //        throw new InvalidDescriptionException(ex, "load is of wrong type");
            //    }
            //    catch (Exception ex)
            //    {
            //        throw new InvalidDescriptionException(ex, "load is not a valid choice");
            //    }
            //
            //}
            //
            //if ((map["authors"] != null))
            //{
            //    ImmutableList.Builder<String> authorsBuilder = ImmutableList.<String > builder());
            //
            //    if ((map["author"] != null))
            //    {
            //        authorsBuilder.add(map["author"].ToString());
            //    }
            //
            //    try
            //    {
            //        foreach (object o in ((Iterable)(map["authors"])))
            //        {
            //            authorsBuilder.add(o.ToString());
            //        }
            //
            //    }
            //    catch (Exception ex)
            //    {
            //        throw new InvalidDescriptionException(ex, "authors are of wrong type");
            //    }
            //    //catch (Exception ex)
            //    //{
            //    //    throw new InvalidDescriptionException(ex, "authors are improperly defined");
            //    //}
            //
            //    authors = authorsBuilder.build();
            //}
            //else if ((map["author"] != null))
            //{
            //    authors = ImmutableList.of(map["author"].ToString());
            //}
            //else
            //{
            //    authors = ImmutableList.<;
            //    (String > of());
            //}

            //if ((map["default-permission"] != null))
            //{
            //    try
            //    {
            //        defaultPerm = PermissionDefault.getByName(map["default-permission"].ToString());
            //    }
            //    catch (Exception ex)
            //    {
            //        throw new InvalidDescriptionException(ex, "default-permission is of wrong type");
            //    }
            //    catch (Exception ex)
            //    {
            //        throw new InvalidDescriptionException(ex, "default-permission is not a valid choice");
            //    }
            //
            //}
            //
            //if ((map["awareness") is Iterable))
            //{
            //    Set<PluginAwareness> awareness = new HashSet<PluginAwareness>();
            //    try
            //    {
            //        foreach (Object o in ((Iterable)(map["awareness"))))
            //        {
            //            awareness.add(((PluginAwareness)(o)));
            //        }
            //
            //    }
            //    catch (Exception ex)
            //    {
            //        throw new InvalidDescriptionException(ex, "awareness has wrong type");
            //    }
            //
            //    this.awareness = ImmutableSet.copyOf(awareness);
            //}
            //
            //try
            //{
            //    lazyPermissions = ((Map)(map["permissions")));
            //}
            //catch (Exception ex)
            //{
            //    throw new InvalidDescriptionException(ex, "permissions are of the wrong type");
            //}

            if ((map["prefix"] != null))
            {
                Prefix = map["prefix"].ToString();
            }
        }
    }
}