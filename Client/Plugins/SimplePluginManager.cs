using System;
using System.Collections.Generic;
using System.IO;
using API;
using API.Exceptions;
using API.Helper;
using API.Plugins;
using API.Plugins.Interfaces;

namespace Client.Plugins
{
    public sealed class SimplePluginManager : IPluginManager
    {

        private readonly IClient server;

        private Dictionary<string, IPluginLoader> fileAssociations = new Dictionary<string, IPluginLoader>();

        private List< IPlugin> plugins = new List< IPlugin>();

        private Dictionary<string, IPlugin> lookupNames = new Dictionary<string, IPlugin>();

        private static DirectoryInfo updateDirectory = null;

        //private SimpleCommandMap commandMap;

        //private Dictionary<string, Permission> permissions = new Dictionary<string, Permission>();
        //
        //private Dictionary<Boolean, List<Permission>> defaultPerms = new LinkedDictionary<Boolean, List<Permission>>();
        //
        //private Dictionary<string, Dictionary<Permissible, Boolean>> permSubs = new Dictionary<string, Dictionary<Permissible, Boolean>>();
        //
        //private Dictionary<Boolean, Dictionary<Permissible, Boolean>> defSubs = new Dictionary<Boolean, Dictionary<Permissible, Boolean>>();

        private bool _useTimings = false;

        public SimplePluginManager(IClient instance, object commandMap)
        {
            server = instance;
            ///this.commandMap = this.commandMap;
            ///this.defaultPerms.Add(true, new List<Permission>());
            ///this.defaultPerms.Add(false, new List<Permission>());
        }

        //public void registerInterface(IPluginLoader loader) {
        //    IPluginLoader instance;
        //    if (PluginLoader.class.isAssignableFrom(loader)) {
        //        Constructor<PluginLoader> constructor;
        //        try {
        //            constructor = loader.getConstructor(Server.class);
        //            instance = constructor.newInstance(this.server);
        //        }
        //        catch (NoSuchMethodException ex) {
        //            string className = loader.getName();
        //            throw new IllegalArgumentException(string.Format("Class %s does not have a public %s(Server) constructor", className, className), ex);
        //        }
        //        catch (Exception ex) {
        //            throw new IllegalArgumentException(string.Format("Unexpected exception %s while attempting to construct a new instance of %s", ex.getClass().getName(), loader.getName()), ex);
        //        }
        //    
        //    }
        //    else {
        //        throw new IllegalArgumentException(string.Format("Class %s does not implement interface PluginLoader", loader.getName()));
        //    }
        //
        //    Pattern[] patterns = instance.getPluginFileFilters();
        //    this;
        //    foreach (Pattern pattern in patterns) {
        //        this.fileAssociations.Add(pattern, instance);
        //    }
        //
        //}

        /// <inheritdoc/>
        public void RegisterInterface(IPluginLoader loader)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public IPlugin GetPlugin(string name)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public IPlugin[] GetPlugins()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public bool IsPluginEnabled(string name)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public bool IsPluginEnabled(IPlugin plugin)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public IPlugin LoadPlugin(FileInfo file)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public IPlugin[] LoadPlugins(DirectoryInfo directory)
        {
            Validate.NotNull(directory, "Directory cannot be null");
            Validate.IsTrue(directory.Exists, "Directory must be a directory");
            var result = new List<IPlugin>();

            //List<Pattern> filters = fileAssociations.keySet();

            //if (!this.server.getUpdateFolder().equals("")) {
            //    updateDirectory = new File(directory, this.server.getUpdateFolder());
            //}

            var _plugins = new Dictionary<string, FileInfo>();
            var loadedPlugins = new List<string>();
            var dependencies = new Dictionary<string, List<string>>();
            var softDependencies = new Dictionary<string, List<string>>();

            //  This is where it figures out all possible plugins
            foreach (FileInfo file in directory.GetFiles())
            {
                IPluginLoader loader = new DotNetPluginLoader(server);

                //foreach (Pattern filter in filters) {
                //    Matcher match = filter.matcher(file.getName());
                //    if (match.find()) {
                //        loader = this.fileAssociations.get(filter);
                //    }
                //}
                //

                if (loader == null)
                {
                    // TODO: Warning!!! continue If
                }

                PluginDescriptionFile description = null;

                try
                {
                    description = loader.GetPluginDescription(file);
                    string name = description.GetName();
                    //if ((name.Equals("bukkit") 
                    //     || (name.Equals("minecraft") || name.Equals("mojang")))) {
                    //    this.server.GetLogger().Error( ("Could not load \'" 
                    //                                               + (file.FullName + ("\' in folder \'" 
                    //                                                   + (directory.FullName + "\': Restricted Name")))));
                    //    // TODO: Warning!!! continue If
                    //}
                    //else
                    if ((description.GetRawName().IndexOf(' ') != -1))
                    {
                        server.GetLogger().Warn(string.Format("Plugin `%s\' uses the space-character (0x20) in its name `%s\' - this is discouraged", description.GetFullName(), description.RawName));
                    }
                }
                catch (InvalidDescriptionException ex)
                {
                    server.GetLogger().Error(("Could not load \'"
                                                               + (file.FullName + ("\' in folder \'"
                                                                   + (directory.FullName + "\'")))), ex);
                    // TODO: Warning!!! continue Catch
                    continue;
                }

                _plugins.Add(description.GetName(), file);

                //FileInfo replacedFile = file;
                //
                //if ((replacedFile != null))
                //{
                //    server.GetLogger().Error(string.Format("Ambiguous plugin name `%s\' for files `%s\' and `%s\' in `%s\'", description.GetName(), file.FullName, replacedFile.FullName, directory.FullName));
                //}

                //Collection<string> softDependencySet = description.GetSoftDepend();
                //
                //if (((softDependencySet != null) 
                //     && softDependencySet.Count != 0)) {
                //    if (softDependencies.ContainsKey(description.GetName())) {
                //        //  Duplicates do not matter, they will be removed together if applicable
                //        softDependencies[description.GetName()].AddRange(softDependencySet);
                //    }
                //    else {
                //        softDependencies.Add(description.getName(), new List<string>(softDependencySet));
                //    }
                //
                //}
                //
                //Collection<string> dependencySet = description.getDepend();
                //if (((dependencySet != null) 
                //     && !dependencySet.isEmpty())) {
                //    dependencies.Add(description.getName(), new List<string>(dependencySet));
                //}
                //
                //Collection<string> loadBeforeSet = description.getLoadBefore();
                //if (((loadBeforeSet != null) 
                //     && !loadBeforeSet.isEmpty())) {
                //    foreach (string loadBeforeTarget in loadBeforeSet) {
                //        if (softDependencies.ContainsKey(loadBeforeTarget)) {
                //            softDependencies.get(loadBeforeTarget).Add(description.getName());
                //        }
                //        else {
                //            //  softDependencies is never iterated, so 'ghost' plugins aren't an issue
                //            Collection<string> shortSoftDependency = new List<string>();
                //            shortSoftDependency.Add(description.getName());
                //            softDependencies.Add(loadBeforeTarget, shortSoftDependency);
                //        }
                //    
                //    }
                //
                //}

            }

            while (_plugins.Count > 0)
            {
                bool missingDependency = true;
                List<string> pluginIteratorRemove = new List<string>();

                var pluginIterator = _plugins.Keys.GetEnumerator();
                while (pluginIterator.MoveNext())
                {
                    string plugin = pluginIterator.Current;
                    //        if (dependencies.ContainsKey(plugin)) {
                    //            Iterator<string> dependencyIterator = dependencies.get(plugin).iterator();
                    //            while (dependencyIterator.hasNext()) {
                    //                string dependency = dependencyIterator.next();
                    //                //  Dependency loaded
                    //                if (loadedPlugins.contains(dependency)) {
                    //                    dependencyIterator.Remove();
                    //                    //  We have a dependency not found
                    //                }
                    //                else if (!this.plugins.ContainsKey(dependency)) {
                    //                    missingDependency = false;
                    //                    FileInfo FileInfo = this.plugins.get(plugin);
                    //                    pluginIterator.Remove();
                    //                    softDependencies.Remove(plugin);
                    //                    dependencies.Remove(plugin);
                    //                    this.server.GetLogger().Error( ("Could not load \'" 
                    //                                                               + (file.FullName + ("\' in folder \'" 
                    //                                                                   + (directory.FullName + "\'")))), new UnknownDependencyException(dependency));
                    //                    break;
                    //                }
                    //            
                    //            }
                    //        
                    //            if ((dependencies.ContainsKey(plugin) && dependencies.get(plugin).isEmpty())) {
                    //                dependencies.Remove(plugin);
                    //            }
                    //        
                    //        }
                    //    
                    //        if (softDependencies.ContainsKey(plugin)) {
                    //            Iterator<string> softDependencyIterator = softDependencies.get(plugin).iterator();
                    //            while (softDependencyIterator.hasNext()) {
                    //                string softDependency = softDependencyIterator.next();
                    //                //  Soft depend is no longer around
                    //                if (!this.plugins.ContainsKey(softDependency)) {
                    //                    softDependencyIterator.Remove();
                    //                }
                    //            
                    //            }
                    //        
                    //            if (softDependencies.get(plugin).isEmpty()) {
                    //                softDependencies.Remove(plugin);
                    //            }
                    //        
                    //        }
                    //    
                    if ((!(dependencies.ContainsKey(plugin) || softDependencies.ContainsKey(plugin)) && _plugins.ContainsKey(plugin)))
                    {
                        //  We're clear to load, no more soft or hard dependencies left
                        FileInfo file = _plugins[plugin];

                        pluginIteratorRemove.Add(plugin);
                        //pluginIterator.Remove();
                        missingDependency = false;

                        try
                        {
                            result.Add(loadPlugin(file));
                            loadedPlugins.Add(plugin);
                            // TODO: Warning!!! continue Try
                        }
                        catch (InvalidPluginException ex)
                        {
                            server.GetLogger().Error($"Could not load '{file.FullName}' in folder '{directory.FullName}'", ex);
                        }
                    }
                    //    
                    //    }
                    //
                    //    if (missingDependency) {
                    //        //  We now iterate over plugins until something loads
                    //        //  This loop will ignore soft dependencies
                    //        pluginIterator = this.plugins.GetEnumerator();
                    //        while (pluginIterator.hasNext()) {
                    //            string plugin = pluginIterator.next();
                    //            if (!dependencies.ContainsKey(plugin)) {
                    //                softDependencies.Remove(plugin);
                    //                missingDependency = false;
                    //                FileInfo file = this.plugins[plugin];
                    //                pluginIterator.Remove();
                    //                try {
                    //                    result.Add(this.loadPlugin(file));
                    //                    loadedPlugins.Add(plugin);
                    //                    break;
                    //                }
                    //                catch (InvalidPluginException ex) {
                    //                    this.server.GetLogger().Error( ("Could not load \'" 
                    //                                                               + (file.FullName + ("\' in folder \'" 
                    //                                                                   + (directory.FullName + "\'")))), ex);
                    //                }
                    //            
                    //            }
                    //        
                    //        }
                    //    
                    //        //  We have no plugins left without a depend
                    //        if (missingDependency) {
                    //            softDependencies.Clear();
                    //            dependencies.Clear();
                    //            Enumerator<FileInfo> failedPluginIterator = this.plugins.Values().iterator();
                    //            while (failedPluginIterator.hasNext()) {
                    //                FileInfo file = failedPluginIterator.next();
                    //                failedPluginIterator.Remove();
                    //                this.server.GetLogger().Error( ("Could not load \'" 
                    //                                                           + (file.FullName + ("\' in folder \'" 
                    //                                                               + (directory.FullName + "\': circular dependency detected")))));
                    //            }
                    //        
                    //        }
                    //    
                }

                foreach (var plugin in pluginIteratorRemove)
                {
                    _plugins.Remove(plugin);
                }

            }

            return result.ToArray();
        }

        /// <inheritdoc/>
        public void DisablePlugins()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void ClearPlugins()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void EnablePlugin(IPlugin plugin)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void DisablePlugin(IPlugin plugin)
        {
            throw new NotImplementedException();
        }

        public IPlugin loadPlugin(FileInfo file) {
            Validate.NotNull(file, "FileInfo cannot be null");
            //this.checkUpdate(file);
            //List<Pattern> filters = this.fileAssociations.keySet();
            IPlugin result = null;
            
            //foreach (Pattern filter in filters) {
            //    string name = file.getName();
            //    Matcher match = filter.matcher(name);
            //    if (match.find()) {
                    IPluginLoader loader = new DotNetPluginLoader(server); //this.fileAssociations.get(filter);
            result = loader.LoadPlugin(file);
                    loader.EnablePlugin(result);
            //    }
            //
            //}

            if (result != null) {
                plugins.Add(result);
                lookupNames.Add(result.GetDescription().GetName(), result);
            }
        
            return result;
        }
        
        //private void checkUpdate(FileInfo file) {
        //    if (((updateDirectory == null) 
        //         || !updateDirectory.isDirectory())) {
        //        return;
        //    }
        //
        //    FileInfo updateFileInfo = new File(updateDirectory, file.getName());
        //    if ((updateFile.isFile() && FileUtil.copy(updateFile, file))) {
        //        updateFile.delete();
        //    }
        //
        //}
        //
        //public Plugin getPlugin(string name) {
        //    return this.lookupNames.get(name.replace(' ', '_'));
        //}
        //
        //public Plugin[] getPlugins() {
        //    return this.plugins.toArray(new Plugin[0]);
        //}
        //
        //public bool isPluginEnabled(string name) {
        //    Plugin plugin = this.getPlugin(name);
        //    return this.isPluginEnabled(plugin);
        //}
        //
        //public bool isPluginEnabled(Plugin plugin) {
        //    if (((plugin != null) 
        //         && this.plugins.contains(plugin))) {
        //        return plugin.isEnabled();
        //    }
        //    else {
        //        return false;
        //    }
        //
        //}
        //
        //public void enablePlugin(Plugin plugin) {
        //    if (!plugin.isEnabled()) {
        //        List<Command> pluginCommands = PluginCommandYamlParser.parse(plugin);
        //        if (!pluginCommands.isEmpty()) {
        //            this.commandMap.registerAll(plugin.getDescription().getName(), pluginCommands);
        //        }
        //    
        //        try {
        //            plugin.getPluginLoader().enablePlugin(plugin);
        //        }
        //        catch (Throwable ex) {
        //            this.server.GetLogger().Error( ("Error occurred (in the plugin loader) while enabling " 
        //                                                       + (plugin.getDescription().getFullName() + " (Is it up to date?)")), ex);
        //        }
        //    
        //        HandlerList.bakeAll();
        //    }
        //
        //}
        //
        //public void disablePlugins() {
        //    Plugin[] plugins = this.getPlugins();
        //    for (int i = (this.plugins.length - 1); (i >= 0); i--) {
        //        this.disablePlugin(this.plugins[i]);
        //    }
        //
        //}
        //
        //public void disablePlugin(Plugin plugin) {
        //    if (plugin.isEnabled()) {
        //        try {
        //            plugin.getPluginLoader().disablePlugin(plugin);
        //        }
        //        catch (Throwable ex) {
        //            this.server.GetLogger().Error( ("Error occurred (in the plugin loader) while disabling " 
        //                                                       + (plugin.getDescription().getFullName() + " (Is it up to date?)")), ex);
        //        }
        //    
        //        try {
        //            this.server.getScheduler().cancelTasks(plugin);
        //        }
        //        catch (Throwable ex) {
        //            this.server.GetLogger().Error( ("Error occurred (in the plugin loader) while cancelling tasks for " 
        //                                                       + (plugin.getDescription().getFullName() + " (Is it up to date?)")), ex);
        //        }
        //    
        //        try {
        //            this.server.getServicesManager().unregisterAll(plugin);
        //        }
        //        catch (Throwable ex) {
        //            this.server.GetLogger().Error( ("Error occurred (in the plugin loader) while unregistering services for " 
        //                                                       + (plugin.getDescription().getFullName() + " (Is it up to date?)")), ex);
        //        }
        //    
        //        try {
        //            HandlerList.unregisterAll(plugin);
        //        }
        //        catch (Throwable ex) {
        //            this.server.GetLogger().Error( ("Error occurred (in the plugin loader) while unregistering events for " 
        //                                                       + (plugin.getDescription().getFullName() + " (Is it up to date?)")), ex);
        //        }
        //    
        //        try {
        //            this.server.getMessenger().unregisterIncomingPluginChannel(plugin);
        //            this.server.getMessenger().unregisterOutgoingPluginChannel(plugin);
        //        }
        //        catch (Throwable ex) {
        //            this.server.GetLogger().Error( ("Error occurred (in the plugin loader) while unregistering plugin channels for " 
        //                                                       + (plugin.getDescription().getFullName() + " (Is it up to date?)")), ex);
        //        }
        //    
        //    }
        //
        //}
        //
        //public void clearPlugins() {
        //    this;
        //    this.disablePlugins();
        //    this.plugins.Clear();
        //    this.lookupNames.Clear();
        //    HandlerList.unregisterAll();
        //    this.fileAssociations.Clear();
        //    this.permissions.Clear();
        //    this.defaultPerms.get(true).Clear();
        //    this.defaultPerms.get(false).Clear();
        //}
        //
        //public void callEvent(Event event) {
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
        //    this.fireEvent(event);
        //}
        //
        //private void fireEvent(Event event) {
        //    HandlerList handlers;
        //    getHandlers();
        //    RegisteredListener[] listeners = handlers.getRegisteredListeners();
        //    foreach (RegisteredListener registration in listeners) {
        //        if (!registration.getPlugin().isEnabled()) {
        //            // TODO: Warning!!! continue If
        //        }
        //    
        //        try {
        //            registration.callEvent(event);
        //        }
        //        catch (AuthorNagException ex) {
        //            Plugin plugin = registration.getPlugin();
        //            if (plugin.isNaggable()) {
        //                plugin.setNaggable(false);
        //                this.server.GetLogger().Error( string.Format("Nag author(s): \'%s\' of \'%s\' about the following: %s", plugin.getDescription().getAuthors(), plugin.getDescription().getFullName(), ex.getMessage()));
        //            }
        //        
        //        }
        //        catch (Throwable ex) {
        //            this.server.GetLogger().Error( Could not pass event , +, event, ., (getEventName() + (" to " + registration.getPlugin().getDescription().getFullName())), ex);
        //        }
        //    
        //    }
        //
        //}
        //
        //public void registerEvents(Listener listener, Plugin plugin) {
        //    if (!plugin.isEnabled()) {
        //        throw new IllegalPluginAccessException(("Plugin attempted to register " 
        //                                                + (listener + " while not enabled")));
        //    }
        //
        //    foreach (Map.Entry<Class<Event>, List<RegisteredListener>> entry in plugin.getPluginLoader().createRegisteredListeners(listener, plugin).entrySet()) {
        //        this.getEventListeners(this.getRegistrationClass(entry.getKey())).registerAll(entry.getValue());
        //    }
        //
        //}
        //
        //public void registerEvent(Class<Event> event, Listener listener, EventPriority priority, EventExecutor executor, Plugin plugin) {
        //    this.registerEvent(event, listener, priority, executor, plugin, false);
        //}
        //
        //public void registerEvent(Class<Event> event, Listener listener, EventPriority priority, EventExecutor executor, Plugin plugin, bool ignoreCancelled) {
        //    Validate.notNull(listener, "Listener cannot be null");
        //    Validate.notNull(priority, "Priority cannot be null");
        //    Validate.notNull(executor, "Executor cannot be null");
        //    Validate.notNull(plugin, "Plugin cannot be null");
        //    if (!plugin.isEnabled()) {
        //        throw new IllegalPluginAccessException(Plugin attempted to register , +, event, " while not enabled");
        //    }
        //
        //    if (this.useTimings) {
        //        this.getEventListeners(event).register(new TimedRegisteredListener(listener, executor, priority, plugin, ignoreCancelled));
        //    }
        //    else {
        //        this.getEventListeners(event).register(new RegisteredListener(listener, executor, priority, plugin, ignoreCancelled));
        //    }
        //
        //}
        //
        //private HandlerList getEventListeners(Class<Event> type) {
        //    try {
        //        Method method = this.getRegistrationClass(type).getDeclaredMethod("getHandlerList");
        //        method.setAccessible(true);
        //        return ((HandlerList)(method.invoke(null)));
        //    }
        //    catch (Exception e) {
        //        throw new IllegalPluginAccessException(e.tostring());
        //    }
        //
        //}
        //
        //private Class<Event> getRegistrationClass(Class<Event> clazz) {
        //    try {
        //        clazz.getDeclaredMethod("getHandlerList");
        //        return clazz;
        //    }
        //    catch (NoSuchMethodException e) {
        //        if (((clazz.getSuperclass() != null) 
        //             && (!clazz.getSuperclass().equals(Event.class) 
        //            && Event.class.isAssignableFrom(clazz.getSuperclass())))) {
        //            return this.getRegistrationClass(clazz.getSuperclass().asSubclass(Event.class));
        //        }
        //        else {
        //            throw new IllegalPluginAccessException(("Unable to find handler list for event " + clazz.getName()));
        //        }
        //    
        //    }
        //
        //}
        //
        //public Permission getPermission(string name) {
        //    return this.permissions.get(name.toLowerCase());
        //}
        //
        //public void addPermission(Permission perm) {
        //    string name = perm.getName().toLowerCase();
        //    if (this.permissions.ContainsKey(name)) {
        //        throw new IllegalArgumentException(("The permission " 
        //                                            + (name + " is already defined!")));
        //    }
        //
        //    this.permissions.Add(name, perm);
        //    this.calculatePermissionDefault(perm);
        //}
        //
        //public List<Permission> getDefaultPermissions(bool op) {
        //    return ImmutableSet.copyOf(this.defaultPerms.get(op));
        //}
        //
        //public void removePermission(Permission perm) {
        //    this.RemovePermission(perm.getName());
        //}
        //
        //public void removePermission(string name) {
        //    this.permissions.Remove(name.toLowerCase());
        //}
        //
        //public void recalculatePermissionDefaults(Permission perm) {
        //    if (this.permissions.containsValue(perm)) {
        //        this.defaultPerms.get(true).Remove(perm);
        //        this.defaultPerms.get(false).Remove(perm);
        //        this.calculatePermissionDefault(perm);
        //    }
        //
        //}
        //
        //private void calculatePermissionDefault(Permission perm) {
        //    if (((perm.getDefault() == PermissionDefault.OP) 
        //         || (perm.getDefault() == PermissionDefault.TRUE))) {
        //        this.defaultPerms.get(true).Add(perm);
        //        this.dirtyPermissibles(true);
        //    }
        //
        //    if (((perm.getDefault() == PermissionDefault.NOT_OP) 
        //         || (perm.getDefault() == PermissionDefault.TRUE))) {
        //        this.defaultPerms.get(false).Add(perm);
        //        this.dirtyPermissibles(false);
        //    }
        //
        //}
        //
        //private void dirtyPermissibles(bool op) {
        //    List<Permissible> permissibles = this.getDefaultPermSubscriptions(op);
        //    foreach (Permissible p in permissibles) {
        //        p.recalculatePermissions();
        //    }
        //
        //}
        //
        //public void subscribeToPermission(string permission, Permissible permissible) {
        //    string name = permission.toLowerCase();
        //    Dictionary<Permissible, Boolean> map = this.permSubs.get(name);
        //    if ((map == null)) {
        //        map = new WeakDictionary<Permissible, Boolean>();
        //        this.permSubs.Add(name, map);
        //    }
        //
        //    map.Add(permissible, true);
        //}
        //
        //public void unsubscribeFromPermission(string permission, Permissible permissible) {
        //    string name = permission.toLowerCase();
        //    Dictionary<Permissible, Boolean> map = this.permSubs.get(name);
        //    if ((map != null)) {
        //        map.Remove(permissible);
        //        if (map.isEmpty()) {
        //            this.permSubs.Remove(name);
        //        }
        //    
        //    }
        //
        //}
        //
        //public List<Permissible> getPermissionSubscriptions(string permission) {
        //    string name = permission.toLowerCase();
        //    Dictionary<Permissible, Boolean> map = this.permSubs.get(name);
        //    if ((map == null)) {
        //        return ImmutableSet.of();
        //    }
        //    else {
        //        return ImmutableSet.copyOf(map.keySet());
        //    }
        //
        //}
        //
        //public void subscribeToDefaultPerms(bool op, Permissible permissible) {
        //    Dictionary<Permissible, Boolean> map = this.defSubs.get(op);
        //    if ((map == null)) {
        //        map = new WeakDictionary<Permissible, Boolean>();
        //        this.defSubs.Add(op, map);
        //    }
        //
        //    map.Add(permissible, true);
        //}
        //
        //public void unsubscribeFromDefaultPerms(bool op, Permissible permissible) {
        //    Dictionary<Permissible, Boolean> map = this.defSubs.get(op);
        //    if ((map != null)) {
        //        map.Remove(permissible);
        //        if (map.isEmpty()) {
        //            this.defSubs.Remove(op);
        //        }
        //    
        //    }
        //
        //}
        //
        //public List<Permissible> getDefaultPermSubscriptions(bool op) {
        //    Dictionary<Permissible, Boolean> map = this.defSubs.get(op);
        //    if ((map == null)) {
        //        return ImmutableSet.of();
        //    }
        //    else {
        //        return ImmutableSet.copyOf(map.keySet());
        //    }
        //
        //}
        //
        //public List<Permission> getPermissions() {
        //    return new List<Permission>(this.permissions.values());
        //}
        //

        /// <inheritdoc/>
        public bool UseTimings()
        {
            return _useTimings;
        }

        public void UseTimings(bool use)
        {
            _useTimings = use;
        }
    }
}