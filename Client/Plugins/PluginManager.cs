using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;
using System.Threading;
using API;
using API.Chat;
using API.Commands;
using API.Exceptions;
using API.Helper;
using API.Network;
using API.Plugins;
using API.Plugins.Interfaces;
using Client.Client;
using Client.Database;
using Client.Phrase;
using Client.Property;

namespace Client.Plugins
{
    public sealed class PluginManager : IPluginManager
    {
        private readonly IClient _client;

        private readonly Dictionary<string, IPluginLoader> _fileAssociations = new Dictionary<string, IPluginLoader>();

        private readonly List<IPlugin> _plugins = new List<IPlugin>();

        private readonly Dictionary<string, IPlugin> _lookupNames = new Dictionary<string, IPlugin>();

        private readonly Dictionary<IPlugin, List<ListenerBase>> _listenerSlots = new Dictionary<IPlugin, List<ListenerBase>>();

        public PluginManager(IClient instance)
        {
            _client = instance;
        }

        /// <inheritdoc/>
        public void RegisterInterface(Type loader)
        {
            IPluginLoader instance;

            try
            {
                instance = (IPluginLoader)Activator.CreateInstance(loader, _client);
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
                    if (!new Regex(filter).IsMatch(file.Name))
                        continue;

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

                    if (description.GetRawName().IndexOf(' ') != -1)
                    {
                        _client.GetLogger().Warn($"Plugin '{description.GetFullName()}' uses the space-character (0x20) in its name '{description.RawName}' - this is discouraged");
                    }
                }
                catch (InvalidDescriptionException ex)
                {
                    _client.GetLogger().Warn($"Could not load '{file.Name}' in folder '{directory.Name}'. Embedded resource 'plugin.yml' may be missing.", ex);
                    continue;
                }

                if (plugins.ContainsKey(description.GetName()))
                {
                    _client.GetLogger().Error($"Ambiguous plugin name '{description.GetName()}' for files '{file.FullName}' and '{plugins[description.GetName()].FullName}' in '{directory.FullName}'");
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
                                    dependencies.Remove(plugin);
                                }
                                else if (!plugins.ContainsKey(dependency))
                                {
                                    missingDependency = false;
                                    var file = plugins[plugin];

                                    plugins.Remove(plugin);

                                    softDependencies.Remove(plugin);
                                    dependencies.Remove(plugin);
                                    _client.GetLogger().Error($"Could not load '{file.FullName}' in folder '{directory.FullName}'", new UnknownDependencyException(dependency));
                                    break;
                                }
                            }

                            if (dependencies.ContainsKey(plugin) && dependencies[plugin].Count == 0)
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

                            plugins.Remove(plugin);

                            missingDependency = false;

                            try
                            {
                                result.Add(LoadPlugin(file));
                                loadedPlugins.Add(plugin);
                            }
                            catch (InvalidPluginException ex)
                            {
                                _client.GetLogger().Error($"Could not load '{file.FullName}' in folder '{directory.FullName}'", ex);
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

                            plugins.Remove(plugin);

                            try
                            {
                                result.Add(LoadPlugin(file));
                                loadedPlugins.Add(plugin);
                                break;
                            }
                            catch (InvalidPluginException ex)
                            {
                                _client.GetLogger().Error($"Could not load '{file.FullName}' in folder '{directory.FullName}'", ex);
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

                            plugins.Remove(pluginIterator.Current);

                            _client.GetLogger().Error($"Could not load '{file.FullName}' in folder '{directory.FullName}': circular dependency detected");
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
                _client.GetLogger().Error($"Error occurred (in the plugin loader) while enabling {plugin.GetDescription().GetFullName()} (Is it up to date?)", ex);
            }

            // TODO: add listeners again?
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
                _client.GetLogger().Error($"Error occurred (in the plugin loader) while disabling {plugin.GetDescription().GetFullName()} (Is it up to date?)", ex);
            }

            // TODO: remove listeners?
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

        public void CallEvent(Action<ListenerBase> evt)
        {
            FireEvent(evt);
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
                    catch (Exception e)
                    {
                        _client.GetLogger().Error($"Could not pass event {action.Method.Name} to {plugin.GetDescription().GetFullName()}\r\n{e.Message}\r\n{e.StackTrace}");
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
                    DispatchListenerEvent(listener => listener.OnInitialize(), new[] { (ListenerBase)newListener });

                if (_client.IsInGame())
                    DispatchListenerEvent(listener => listener.OnGameJoined(), new[] { (ListenerBase)newListener });
            }
        }

        #region Event API

        /// <summary>
        /// Called when the client disconnects from the server
        /// </summary>
        public void OnDisconnect()
        {
            DispatchListenerEvent(listener => listener.OnDisconnect(ListenerBase.DisconnectReason.UserLogout, ""));
        }

        /// <summary>
        /// Called after the connection was unrecoverable lost
        /// </summary>
        public void OnConnectionLost(ListenerBase.DisconnectReason reason, string message)
        {
            DispatchListenerEvent(listener => listener.OnDisconnect(reason, message));
        }

        /// <summary>
        /// Called after an internal command has been performed
        /// </summary>
        public void OnInternalCommand(string commandName, string command, string responseMsg)
        {
            DispatchListenerEvent(listener => listener.OnInternalCommand(commandName, string.Join(" ", CommandBase.GetArgs(command)), responseMsg));
        }

        /// <summary>
        /// Called from the main instance to update all the listener every some ticks
        /// </summary>
        public void OnUpdate()
        {
            try
            {
                DispatchListenerEvent(listener => listener.OnUpdate());
            }
            catch (Exception e)
            {
                if (!(e is ThreadAbortException))
                {
                    _client.GetLogger().Warn($"OnUpdate: Got error: {e}");
                }
                else throw; //ThreadAbortException should not be caught
            }

            foreach (var plugin in _plugins.ToArray())
            {
                try
                {
                    plugin.UpdateInternal();
                }
                catch (Exception e)
                {
                    if (!(e is ThreadAbortException))
                    {
                        _client.GetLogger().Warn($"OnUpdate: Got error from {plugin}: {e}");
                    }
                    else throw; //ThreadAbortException should not be caught
                }
            }
        }

        /// <summary>
        /// Called when a server was successfully joined
        /// </summary>
        public void OnGameJoined()
        {
            DispatchListenerEvent(listener => listener.OnGameJoined());
        }

        /// <summary>
        /// Called when one of the characters from the friend list updates
        /// </summary>
        /// <param name="contactId">id</param>
        /// <param name="online">new status</param>
        public void OnTeamContactStatus(uint contactId, CharConnectionState online)
        {
            DispatchListenerEvent(listener => listener.OnTeamContactStatus(contactId, online));
        }

        /// <summary>
        /// Called when friend list and ignore list from the contact list are initialized
        /// </summary>
        internal void OnTeamContactInit(List<uint> vFriendListName, List<CharConnectionState> vFriendListOnline, List<string> vIgnoreListName)
        {
            DispatchListenerEvent(listener => listener.OnTeamContactInit(vFriendListName, vFriendListOnline, vIgnoreListName));
        }

        /// <summary>
        /// Called when one character from the friend or ignore list is created
        /// </summary>
        internal void OnTeamContactCreate(uint contactId, uint nameId, CharConnectionState online, byte nList)
        {
            DispatchListenerEvent(listener => listener.OnTeamContactCreate(contactId, nameId, online, nList));
        }

        /// <summary>
        /// Remove a contact by the server
        /// </summary>
        internal void OnTeamContactRemove(uint contactId, byte nList)
        {
            DispatchListenerEvent(listener => listener.OnTeamContactRemove(contactId, nList));
        }

        /// <summary>
        /// Any chat will arrive here 
        /// </summary>
        internal void OnChat(uint compressedSenderIndex, string ucstr, string rawMessage, ChatGroupType mode, uint dynChatId, string senderName, uint bubbleTimer)
        {
            DispatchListenerEvent(listener => listener.OnChat(compressedSenderIndex, ucstr, rawMessage, mode, dynChatId, senderName, bubbleTimer));
        }

        /// <summary>
        /// Any tells will arrive here 
        /// </summary>
        internal void OnTell(string ucstr, string senderName)
        {
            DispatchListenerEvent(listener => listener.OnTell(ucstr, senderName));
        }

        /// <summary>
        /// called when the server activates/deactivates use of female titles
        /// </summary>
        public void OnGuildUseFemaleTitles(bool useFemaleTitles)
        {
            DispatchListenerEvent(listener => listener.OnGuildUseFemaleTitles(useFemaleTitles));
        }

        /// <summary>
        /// called when the server upload the phrases.
        /// </summary>
        public void OnPhraseDownLoad(List<PhraseSlot> phrases, List<PhraseMemorySlot> memorizedPhrases)
        {
            DispatchListenerEvent(listener => listener.OnPhraseDownLoad(phrases, memorizedPhrases));
        }

        /// <summary>
        /// called when the server block/unblock some reserved titles
        /// </summary>
        public void OnGuildUpdatePlayerTitle(bool unblock, int len, List<ushort> titles)
        {
            DispatchListenerEvent(listener => listener.OnGuildUpdatePlayerTitle(unblock, len, titles));
        }

        /// <summary>
        /// called when the server sends a new respawn point
        /// </summary>
        public void OnDeathRespawnPoint(int x, int y)
        {
            DispatchListenerEvent(listener => listener.OnDeathRespawnPoint(x, y));
        }

        /// <summary>
        /// called when the server sends the encyclopedia initialization
        /// </summary>
        public void OnEncyclopediaInit()
        {
            DispatchListenerEvent(listener => listener.OnEncyclopediaInit());
        }

        /// <summary>
        /// called when the server sends the inventory initialization
        /// </summary>
        public void OnInitInventory(uint serverTick)
        {
            DispatchListenerEvent(listener => listener.OnInitInventory(serverTick));
        }

        /// <summary>
        /// called when the server sends the database initialization
        /// </summary>
        public void OnDatabaseInitPlayer(uint serverTick)
        {
            DispatchListenerEvent(listener => listener.OnDatabaseInitPlayer(serverTick));
        }

        /// <summary>
        /// called when the server sends the database updates
        /// </summary>
        public void OnDatabaseUpdatePlayer(uint serverTick)
        {
            DispatchListenerEvent(listener => listener.OnDatabaseUpdatePlayer(serverTick));
        }

        /// <summary>
        /// called when the server updates the user hp, sap, stamina and focus bars/stats
        /// </summary>
        public void OnUserBars(byte msgNumber, int hp, int sap, int sta, int focus)
        {
            DispatchListenerEvent(listener => listener.OnUserBars(msgNumber, hp, sap, sta, focus));
        }

        /// <summary>
        /// called when a database bank gets initialized
        /// </summary>
        public void OnDatabaseInitBank(uint serverTick, uint bank, DatabaseManager databaseManager)
        {
            DispatchListenerEvent(automaton => automaton.OnDatabaseInitBank(serverTick, bank, databaseManager));
        }

        /// <summary>
        /// called when a database bank gets updated
        /// </summary>
        internal void OnDatabaseUpdateBank(uint serverTick, uint bank, DatabaseManager databaseManager)
        {
            DispatchListenerEvent(automaton => automaton.OnDatabaseUpdateBank(serverTick, bank, databaseManager));
        }

        /// <summary>
        /// called when a database bank gets reset
        /// </summary>
        internal void OnDatabaseResetBank(uint serverTick, uint bank, DatabaseManager databaseManager)
        {
            DispatchListenerEvent(automaton => automaton.OnDatabaseResetBank(serverTick, bank, databaseManager));
        }

        /// <summary>
        /// called when the string cache reloads
        /// </summary>
        public void OnReloadCache(int timestamp)
        {
            DispatchListenerEvent(listener => listener.OnReloadCache(timestamp));
        }

        /// <summary>
        /// called when the local string set updates
        /// </summary>
        public void OnStringResp(uint stringId, string strUtf8)
        {
            DispatchListenerEvent(listener => listener.OnStringResp(stringId, strUtf8));
        }

        /// <summary>
        /// called on local string set updates
        /// </summary>
        public void OnPhraseSend(DynamicStringInfo dynInfo)
        {
            // TODO: OnPhraseSend
            //DispatchAutomatonEvent(automaton => automaton.OnPhraseSend(dynInfo));
        }

        /// <summary>
        /// called when the player gets invited to a team
        /// </summary>
        public void OnTeamInvitation(uint textID)
        {
            DispatchListenerEvent(listener => listener.OnTeamInvitation(textID));
        }

        /// <summary>
        /// called when the server sends information about the user char after the login
        /// </summary>
        public void OnUserChar(int highestMainlandSessionId, int firstConnectedTime, int playedTime, Vector3 initPos, Vector3 initFront, short season, int role, bool isInRingSession)
        {
            DispatchListenerEvent(listener => listener.OnUserChar(highestMainlandSessionId, firstConnectedTime, playedTime, initPos, initFront, season, role, isInRingSession));
        }

        /// <summary>
        /// called when the server sends information about the all the user chars
        /// </summary>
        internal void OnUserChars()
        {
            DispatchListenerEvent(listener => listener.OnUserChars());
        }

        /// <summary>
        /// called when the client receives the shard id and the webhost from the server
        /// </summary>
        public void OnShardID(uint shardId, string webHost)
        {
            DispatchListenerEvent(listener => listener.OnShardID(shardId, webHost));
        }

        /// <summary>
        /// called when an entity health, sap, stamina or focus value changes
        /// </summary>
        public void OnEntityUpdateBars(uint gameCycle, long prop, byte slot, sbyte hitPoints, byte stamina, byte sap, byte focus)
        {
            DispatchListenerEvent(listener => listener.OnEntityUpdateBars(gameCycle, prop, slot, hitPoints, stamina, sap, focus));
        }

        /// <summary>
        /// called when an entity position update occurs
        /// </summary>
        public void OnEntityUpdatePos(uint gameCycle, long prop, byte slot, uint predictedInterval, Vector3 pos)
        {
            DispatchListenerEvent(listener => listener.OnEntityUpdatePos(gameCycle, prop, slot, predictedInterval, pos));
        }

        /// <summary>
        /// called when an entity orientation update occurs
        /// </summary>
        public void OnEntityUpdateOrient(uint gameCycle, long prop)
        {
            DispatchListenerEvent(listener => listener.OnEntityUpdateOrient(gameCycle, prop));
        }

        /// <summary>
        /// Called when the ingame database was received
        /// TODO: we have this two times
        /// </summary>
        internal void OnIngameDatabaseInitialized()
        {
            DispatchListenerEvent(listener => listener.OnIngameDatabaseInitialized());
        }

        /// <summary>
        /// called when an entity is created
        /// </summary>
        public void OnEntityCreate(byte slot, uint form, PropertyChange.TNewEntityInfo newEntityInfo)
        {
            // TODO: OnEntityCreate
            //DispatchAutomatonEvent(automaton => automaton.OnEntityCreate(slot, form, newEntityInfo));
        }

        /// <summary>
        /// called when an entity gets removed
        /// </summary>
        public void OnEntityRemove(byte slot, bool _)
        {
            DispatchListenerEvent(listener => listener.OnEntityRemove(slot));
        }

        /// <summary>
        /// called when visual property is updated
        /// </summary>
        public void OnEntityUpdateVisualProperty(uint gameCycle, byte slot, byte prop, uint predictedInterval)
        {
            DispatchListenerEvent(listener => listener.OnEntityUpdateVisualProperty(gameCycle, slot, prop, predictedInterval));
        }

        #endregion

        /// <summary>
        /// Dispatch a listener event with automatic exception handling
        /// </summary>
        /// <example>
        /// Example for calling SomeEvent() on all automata at once:
        /// DispatchAutomatonEvent(listener => listener.SomeEvent());
        /// </example>
        /// <param name="action">ActionBase to execute on each listener</param>
        /// <param name="listenerList">Only fire the event for the specified listener list (default: all listeners)</param>
        public void DispatchListenerEvent(Action<ListenerBase> action, IEnumerable<ListenerBase> listenerList = null)
        {
            if (listenerList == null)
            {
                // call for all listeners
                CallEvent(action);
                return;
            }

            // call only for specific listeners
            foreach (var listener in listenerList)
            {
                try
                {
                    action(listener);
                }
                catch (Exception e)
                {
                    if (!(e is ThreadAbortException))
                    {
                        //Retrieve parent method name to determine which event caused the exception
                        var frame = new System.Diagnostics.StackFrame(1);
                        var method = frame.GetMethod();
                        var parentMethodName = method?.Name;

                        //Display a meaningful error message to help debugging the listener
                        _client?.GetLogger().Error($"{parentMethodName}: Got error from {listener}: {e}");
                    }
                    // ThreadAbortException should not be caught here as in can happen when disconnecting from server
                    else throw;
                }
            }
        }
    }
}