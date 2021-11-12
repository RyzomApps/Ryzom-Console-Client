using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using API;
using API.Exceptions;
using API.Helper;
using API.Plugins;
using API.Plugins.Interfaces;

namespace Client.Plugins
{
    public sealed class PluginManager : IPluginManager
    {
        private readonly IClient _server;

        private readonly Dictionary<string, IPluginLoader> _fileAssociations = new Dictionary<string, IPluginLoader>();

        private readonly List<IPlugin> _plugins = new List<IPlugin>();

        private readonly Dictionary<string, IPlugin> _lookupNames = new Dictionary<string, IPlugin>();

        private readonly Dictionary<IPlugin, List<ListenerBase>> _listenerSlots = new Dictionary<IPlugin, List<ListenerBase>>();

        public PluginManager(IClient instance)
        {
            _server = instance;
        }

        /// <inheritdoc/>
        public void RegisterInterface(Type loader)
        {
            IPluginLoader instance;

            try
            {
                instance = (IPluginLoader)Activator.CreateInstance(loader, _server);
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Unexpected exception {ex.GetType().Name} while attempting to construct a new instance of {loader}", ex);
            }

            if (instance == null)
            {
                throw new ArgumentException($"Class {loader} does not have a {loader}(Server) constructor");
            }

            var patterns = instance.GetPluginFileFilters();

            foreach (var pattern in patterns)
            {
                _fileAssociations.Add(pattern, instance);
            }
        }

        /// <inheritdoc/>
        public IPlugin[] LoadPlugins(DirectoryInfo directory)
        {
            Validate.NotNull(directory, "Directory cannot be null");
            Validate.IsTrue(directory.Exists, "Directory must be a directory");
            var result = new List<IPlugin>();

            var filters = _fileAssociations.Keys.ToArray();

            var plugins = new Dictionary<string, FileInfo>();
            var loadedPlugins = new List<string>();
            var dependencies = new Dictionary<string, List<string>>();
            var softDependencies = new Dictionary<string, List<string>>();

            //  This is where it figures out all possible plugins
            foreach (var file in directory.GetFiles())
            {
                IPluginLoader loader = null;

                foreach (var filter in filters)
                {
                    if (!new Regex(filter).IsMatch(file.Name)) continue;

                    loader = _fileAssociations[filter];
                }

                if (loader == null)
                    continue;

                PluginDescriptionFile description;

                try
                {
                    description = loader.GetPluginDescription(file);

                    if (description == null)
                    {
                        throw new InvalidDescriptionException("description is empty");
                    }

#pragma warning disable 618
                    if (description.GetRawName().IndexOf(' ') != -1)
#pragma warning restore 618
                    {
                        _server.GetLogger().Warn($"Plugin '{description.GetFullName()}' uses the space-character (0x20) in its name '{description.RawName}' - this is discouraged");
                    }
                }
                catch (InvalidDescriptionException ex)
                {
                    _server.GetLogger().Debug($"Could not load '{file.FullName}' in folder '{directory.FullName}'", ex);
                    continue;
                }

                if (plugins.ContainsKey(description.GetName()))
                {
                    _server.GetLogger().Error($"Ambiguous plugin name '{description.GetName()}' for files '{file.FullName}' and '{plugins[description.GetName()].FullName}' in '{directory.FullName}'");
                    continue;
                }
                else
                {
                    plugins.Add(description.GetName(), file);
                }

                var softDependencySet = description.GetSoftDepend();

                if (softDependencySet != null && softDependencySet.Count != 0)
                {
                    if (softDependencies.ContainsKey(description.GetName()))
                    {
                        // Duplicates do not matter, they will be removed together if applicable
                        softDependencies[description.GetName()].AddRange(softDependencySet);
                    }
                    else
                    {
                        softDependencies.Add(description.GetName(), new List<string>(softDependencySet));
                    }
                }

                var dependencySet = description.GetDepend();

                if (dependencySet != null && dependencySet.Count != 0)
                {
                    dependencies.Add(description.GetName(), new List<string>(dependencySet));
                }

                var loadBeforeSet = description.GetLoadBefore();

                if (loadBeforeSet != null && loadBeforeSet.Count != 0)
                {
                    foreach (var loadBeforeTarget in loadBeforeSet)
                    {
                        if (softDependencies.ContainsKey(loadBeforeTarget))
                        {
                            softDependencies[loadBeforeTarget].Add(description.GetName());
                        }
                        else
                        {
                            // softDependencies is never iterated, so 'ghost' plugins aren't an issue
                            var shortSoftDependency = new List<string> { description.GetName() };
                            softDependencies.Add(loadBeforeTarget, shortSoftDependency);
                        }
                    }
                }
            }

            while (plugins.Count > 0)
            {
                var missingDependency = true;

                var pluginIteratorRemove = new List<string>();

                using (var pluginIterator = plugins.Keys.GetEnumerator())
                {
                    while (pluginIterator.MoveNext())
                    {
                        var plugin = pluginIterator.Current;

                        if (dependencies.ContainsKey(plugin))
                        {
                            using var dependencyIterator = dependencies[plugin].GetEnumerator();

                            while (dependencyIterator.MoveNext())
                            {
                                var dependency = dependencyIterator.Current;

                                //  Dependency loaded
                                if (loadedPlugins.Contains(dependency))
                                {
                                    //  We have a dependency not found
                                    //dependencyIterator.Remove();
                                    dependencies.Remove(plugin);
                                }
                                else if (!plugins.ContainsKey(dependency))
                                {
                                    missingDependency = false;
                                    var file = plugins[plugin];

                                    //pluginIterator.Remove();
                                    plugins.Remove(plugin);

                                    softDependencies.Remove(plugin);
                                    dependencies.Remove(plugin);
                                    _server.GetLogger().Error($"Could not load '{file.FullName}' in folder '{directory.FullName}'", new UnknownDependencyException(dependency));
                                    break;
                                }
                            }

                            if ((dependencies.ContainsKey(plugin) && dependencies[plugin].Count == 0))
                            {
                                dependencies.Remove(plugin);
                            }
                        }

                        if (softDependencies.ContainsKey(plugin))
                        {
                            using var softDependencyIterator = softDependencies[plugin].GetEnumerator();

                            while (softDependencyIterator.MoveNext())
                            {
                                var softDependency = softDependencyIterator.Current;

                                //  Soft depend is no longer around
                                if (!plugins.ContainsKey(softDependency))
                                {
                                    //softDependencyIterator.Remove();
                                    softDependencies.Remove(softDependency);
                                }
                            }

                            if (softDependencies[plugin].Count == 0)
                            {
                                softDependencies.Remove(plugin);
                            }
                        }

                        if (!(dependencies.ContainsKey(plugin) || softDependencies.ContainsKey(plugin)) && plugins.ContainsKey(plugin))
                        {
                            //  We're clear to load, no more soft or hard dependencies left
                            var file = plugins[plugin];

                            pluginIteratorRemove.Add(plugin);

                            //pluginIterator.Remove();
                            plugins.Remove(plugin);

                            missingDependency = false;

                            try
                            {
                                result.Add(LoadPlugin(file));
                                loadedPlugins.Add(plugin);
                            }
                            catch (InvalidPluginException ex)
                            {
                                _server.GetLogger().Error($"Could not load '{file.FullName}' in folder '{directory.FullName}'", ex);
                            }
                        }
                    }
                }

                if (missingDependency)
                {
                    //  We now iterate over plugins until something loads
                    //  This loop will ignore soft dependencies
                    using var pluginIterator = plugins.Keys.GetEnumerator();

                    while (pluginIterator.MoveNext())
                    {
                        var plugin = pluginIterator.Current;

                        if (!dependencies.ContainsKey(plugin))
                        {
                            softDependencies.Remove(plugin);
                            missingDependency = false;
                            var file = plugins[plugin];

                            //pluginIterator.Remove();
                            plugins.Remove(plugin);

                            try
                            {
                                result.Add(LoadPlugin(file));
                                loadedPlugins.Add(plugin);
                                break;
                            }
                            catch (InvalidPluginException ex)
                            {
                                _server.GetLogger().Error($"Could not load '{file.FullName}' in folder '{directory.FullName}'", ex);
                            }
                        }
                    }

                    //  We have no plugins left without a depend
                    if (missingDependency)
                    {
                        softDependencies.Clear();
                        dependencies.Clear();
                        using var failedPluginIterator = plugins.Values.GetEnumerator();

                        while (failedPluginIterator.MoveNext())
                        {
                            var file = failedPluginIterator.Current;

                            //failedPluginIterator.Remove();
                            plugins.Remove(pluginIterator.Current);

                            _server.GetLogger().Error($"Could not load '{file.FullName}' in folder '{directory.FullName}': circular dependency detected");
                        }
                    }
                }

                foreach (var plugin in pluginIteratorRemove)
                {
                    plugins.Remove(plugin);
                }
            }

            return result.ToArray();
        }

        /// <inheritdoc/>
        public IPlugin LoadPlugin(FileInfo file)
        {
            Validate.NotNull(file, "FileInfo cannot be null");

            var filters = _fileAssociations.Keys.ToArray();
            IPlugin result = null;

            foreach (var filter in filters)
            {
                var name = file.Name;

                if (!new Regex(filter).IsMatch(name)) continue;

                var loader = _fileAssociations[filter];

                result = loader.LoadPlugin(file);
                loader.EnablePlugin(result);
            }

            if (result == null) return null;

            _plugins.Add(result);
            _lookupNames.Add(result.GetDescription().GetName(), result);

            return result;
        }

        /// <inheritdoc/>
        public IPlugin GetPlugin(string name)
        {
            return _lookupNames[name.Replace(' ', '_')];
        }

        /// <inheritdoc/>
        public IPlugin[] GetPlugins()
        {
            return _plugins?.ToArray() ?? new IPlugin[0];
        }

        /// <inheritdoc/>
        public bool IsPluginEnabled(string name)
        {
            var plugin = GetPlugin(name);
            return IsPluginEnabled(plugin);
        }

        /// <inheritdoc/>
        public bool IsPluginEnabled(IPlugin plugin)
        {
            if (plugin != null && _plugins.Contains(plugin))
                return plugin.IsEnabled();

            return false;
        }

        /// <inheritdoc/>
        public void EnablePlugin(IPlugin plugin)
        {
            if (plugin.IsEnabled()) return;

            try
            {
                plugin.GetPluginLoader().EnablePlugin(plugin);
            }
            catch (Exception ex)
            {
                _server.GetLogger().Error($"Error occurred (in the plugin loader) while enabling {plugin.GetDescription().GetFullName()} (Is it up to date?)", ex);
            }

            // TODO: Add listeners again?
        }

        /// <inheritdoc/>
        public void DisablePlugins()
        {
            var p = GetPlugins();

            for (var i = p.Length - 1; i >= 0; i--)
            {
                DisablePlugin(p[i]);
            }
        }

        /// <inheritdoc/>
        public void DisablePlugin(IPlugin plugin)
        {
            if (!plugin.IsEnabled()) return;

            try
            {
                plugin.GetPluginLoader().DisablePlugin(plugin);
            }
            catch (Exception ex)
            {
                _server.GetLogger().Error($"Error occurred (in the plugin loader) while disabling {plugin.GetDescription().GetFullName()} (Is it up to date?)", ex);
            }

            // TODO: Remove listeners?
        }

        /// <inheritdoc/>
        public void ClearPlugins()
        {
            DisablePlugins();
            _plugins.Clear();
            _lookupNames.Clear();
            _listenerSlots.Clear();
            _fileAssociations.Clear();
        }

        public void CallEvent(Action<ListenerBase> @event)
        {
            //    isAsynchronous();
            //    if (Thread.holdsLock(this)) {
            //        throw new IllegalStateException(event, ., (getEventName() + " cannot be triggered asynchronously from inside synchronized code."));
            //    }
            //
            //    if (this.server.isPrimaryThread()) {
            //        throw new IllegalStateException(event, ., (getEventName() + " cannot be triggered asynchronously from primary server thread."));
            //    }
            //
            //    this.fireEvent(event);
            //    this;
            FireEvent(@event);
        }

        private void FireEvent(Action<ListenerBase> action)
        {
            foreach (var (plugin, listenerBases) in _listenerSlots)
            {
                foreach (var listener in listenerBases)
                {
                    try
                    {
                        action(listener);
                    }
                    catch
                    {
                        _server.GetLogger().Error($"Could not pass event {action.Method.Name} to {plugin.GetDescription().GetFullName()}");
                    }
                }
            }
        }

        /// <summary>
        /// Register a new listenerBase in this handler list
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public void RegisterListeners(ListenerBase listenerBase, Plugin plugin, bool init = false)
        {
            if (!plugin.IsEnabled())
            {
                throw new IllegalPluginAccessException($"Plugin attempted to register {listenerBase} while not enabled");
            }

            foreach (var newListener in plugin.GetPluginLoader().CreateRegisteredListeners(listenerBase, plugin))
            {
                if (!(newListener is ListenerBase))
                    throw new Exception("This listener is not of type 'ListenerBase'.");

                if (_listenerSlots.ContainsKey(plugin) && _listenerSlots[plugin].Contains(newListener))
                    throw new Exception("This listener is already registered.");

                if (!_listenerSlots.ContainsKey(plugin))
                    _listenerSlots.Add(plugin, new List<ListenerBase>());

                _listenerSlots[plugin].Add((ListenerBase)newListener);

                if (init)
                    ListenerBase.DispatchListenerEvent(listener => listener.OnInitialize(), new[] { (ListenerBase)newListener });

                if (_server.IsInGame())
                    ListenerBase.DispatchListenerEvent(listener => listener.OnGameJoined(), new[] { (ListenerBase)newListener });
            }
        }

        /// <summary>
        /// Called from the main instance to update all the listeners every some ticks
        /// </summary>
        public void OnUpdate()
        {
            ListenerBase.DispatchListenerEvent(listener => listener.OnUpdate(), handler: _server);
        }
    }
}