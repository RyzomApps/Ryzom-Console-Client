﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using API;
using API.Exceptions;
using API.Helper;
using API.Plugins;
using API.Plugins.Interfaces;

namespace Client.Plugins
{
    public sealed class PluginLoader : IPluginLoader
    {
        /// <inheritdoc/>
        public IClient Handler { get; set; }

        private readonly List<string> _fileFilters = new List<string> { @"\.dll$", @"\.nupkg$" };

        private readonly Dictionary<string, PluginClassLoader> _loaders = new Dictionary<string, PluginClassLoader>();

        public PluginLoader(IClient instance)
        {
            Validate.NotNull(instance, "Server cannot be null");
            Handler = instance;
        }

        /// <inheritdoc/>
        public IPlugin LoadPlugin(FileInfo file)
        {
            Validate.NotNull(file, "File cannot be null");

            if (!file.Exists)
            {
                throw new InvalidPluginException(new FileNotFoundException($"{file.FullName} does not exist"));
            }

            PluginDescriptionFile description;
            try
            {
                description = GetPluginDescription(file);
            }
            catch (InvalidDescriptionException ex)
            {
                throw new InvalidPluginException(ex);
            }

            var parentFile = file.Directory?.Parent;
            var dataFolder = new DirectoryInfo($"{parentFile}/plugins/{description.GetName()}");

            if (dataFolder.Exists && (File.GetAttributes(dataFolder.FullName) & FileAttributes.Directory) != FileAttributes.Directory)
            {
                throw new InvalidPluginException($"Projected datafolder: '{dataFolder}' for {description.GetFullName()} ({file}) exists and is not a directory");
            }

            foreach (var pluginName in description.GetDepend())
            {
                if (_loaders == null)
                {
                    throw new UnknownDependencyException(pluginName);
                }

                var current = _loaders[pluginName];

                if (current == null)
                {
                    throw new UnknownDependencyException(pluginName);
                }
            }

            PluginClassLoader loader;

            try
            {
                loader = new PluginClassLoader(this, description, dataFolder, file);
            }
            catch (InvalidPluginException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new InvalidPluginException(ex);
            }

            _loaders.Add(description.GetName(), loader);

            return (Plugin)loader.Class;
        }

        /// <summary>
        /// Extracts a resource file from a different assembly
        /// </summary>
        /// TODO: search for file in every sub namespace
        /// TODO: Move to helper class
        private static string GetResourceFile(string assemblyPath, string fileName)
        {
            var assembly = Assembly.LoadFrom(assemblyPath);
            var nameSpace = Misc.GetAssemblyNamespace(assembly);
            var resourceName = $"{nameSpace}.{fileName}";
            using var stream = assembly.GetManifestResourceStream(resourceName);
            using var reader = new StreamReader(stream ?? throw new InvalidOperationException());

            var resource = reader.ReadToEnd();

            return resource;
        }

        /// <inheritdoc />
        public PluginDescriptionFile GetPluginDescription(FileInfo file)
        {
            Validate.NotNull(file, "File cannot be null");

            try
            {
                var content = GetResourceFile(file.FullName, "plugin.yml");
                return PluginDescriptionFile.Load(content);
            }
            catch (Exception ex)
            {
                throw new InvalidDescriptionException(ex);
            }
        }

        /// <inheritdoc />
        public string[] GetPluginFileFilters()
        {
            return _fileFilters.ToArray();
        }

        /// <inheritdoc />
        public List<IListener> CreateRegisteredListeners(IListener listener, IPlugin plugin)
        {
            Validate.NotNull(plugin, "Plugin can not be null");
            Validate.NotNull(listener, "IListener can not be null");

            return new List<IListener> { listener };
        }

        /// <inheritdoc />
        public void EnablePlugin(IPlugin plugin)
        {
            Validate.IsTrue(plugin is Plugin, "Plugin is not associated with this PluginLoader");
            if (plugin.IsEnabled()) return;

            plugin.GetLogger().Info($"Enabling {plugin.GetDescription().GetFullName()}");

            var csPlugin = (Plugin)(plugin);
            var pluginName = csPlugin.GetDescription().GetName();

            if (!_loaders.ContainsKey(pluginName))
            {
                _loaders.Add(pluginName, csPlugin.GetClassLoader());
            }

            try
            {
              csPlugin.SetEnabled(true);
            }
            catch (Exception e)
            {
                Handler.GetLogger().Error($"Error occurred while enabling {plugin.GetDescription().GetFullName()} (Is it up to date?)\r\n{e.Message}");
            }

            // Perhaps abort here, rather than continue going, but as it stands,
            // an abort is not possible the way it's currently written
            // TODO: Handler.GetPluginManager().CallEvent(new PluginEnableEvent(plugin));
        }

        public void DisablePlugin(IPlugin plugin)
        {
            Validate.IsTrue(plugin is Plugin, "Plugin is not associated with this PluginLoader");
            if (!plugin.IsEnabled()) return;

            var message = $"Disabling {plugin.GetDescription().GetFullName()}";
            plugin.GetLogger().Info(message);

            // TODO: Handler.GetPluginManager().CallEvent(new PluginDisableEvent(plugin));

            var jPlugin = (Plugin)plugin;
            //IClassLoader cloader = jPlugin.GetClassLoader();

            try
            {
                jPlugin.SetEnabled(false);
            }
            catch (Exception)
            {
                Handler.GetLogger().Error($"Error occurred while disabling {plugin.GetDescription().GetFullName()} (Is it up to date?)");
            }

            _loaders.Remove(jPlugin.GetDescription().GetName());

            //if (cloader == null) return;

            //var loader = (PluginClassLoader)cloader;
            //var names = loader.GetClasses();

            //foreach (var name in names)
            //{
            //    this.RemoveClass(name);
            //}
        }
    }
}