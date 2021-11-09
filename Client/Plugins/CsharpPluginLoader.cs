using System;
using System.IO;
using System.Linq;
using System.Reflection;
using API;
using API.Exceptions;
using API.Helper;
using API.Plugins;
using API.Plugins.Interfaces;

namespace Client.Plugins
{
    public sealed class CsharpPluginLoader : IPluginLoader
    {
        public IClient Handler { get; set; }

        //private Pattern[] fileFilters = new Pattern[][][] {Pattern.compile("\\\\.jar$")};
        //
        //private Map<String, Class> classes = new HashMap<String, Class>();

        //private readonly Dictionary<string, PluginClassLoader> _loaders = new Dictionary<string, PluginClassLoader>();

        public CsharpPluginLoader(IClient instance)
        {
            //Validate.NotNull(instance, "Server cannot be null");
            Handler = instance;
        }

        public IPlugin LoadPlugin(FileInfo file)
        {
            Validate.NotNull(file, "File cannot be null");

            if (!file.Exists)
            {
                throw new InvalidPluginException(new FileNotFoundException((file.FullName + " does not exist")));
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
            var dataFolder = new DirectoryInfo(parentFile + "\\" + description.GetName());
            var oldDataFolder = new DirectoryInfo(parentFile + "\\" + description.GetRawName());

            //  Found old data folder
            if (dataFolder.Equals(oldDataFolder))
            {
                //  They are equal -- nothing needs to be done!
            }
            else if ((dataFolder.Exists && oldDataFolder.Exists))
            {
                Handler.GetLogger().Warn(string.Format("While loading %s (%s) found old-data folder: `%s\' next to the new one `%s\'", description.GetFullName(), file, oldDataFolder, dataFolder));
            }
            else if (Directory.Exists(oldDataFolder.Name) && !dataFolder.Exists)
            {
                try
                {
                    File.Move(oldDataFolder.Name, dataFolder.Name);
                }
                catch
                {
                    throw new InvalidPluginException(("Unable to rename old data folder: `" + (oldDataFolder + ("\' to: `" + (dataFolder + "\'")))));
                }

                Handler.GetLogger().Info(string.Format("While loading %s (%s) renamed data folder: `%s\' to `%s\'", description.GetFullName(), file, oldDataFolder, dataFolder));
            }

            if ((dataFolder.Exists && !Directory.Exists(dataFolder.Name)))
            {
                throw new InvalidPluginException(string.Format("Projected datafolder: `%s\' for %s (%s) exists and is not a directory", dataFolder, description.GetFullName(), file));
            }

            //foreach (var pluginName in description.GetDepend())
            //{
            //    if (_loaders == null)
            //    {
            //        throw new UnknownDependencyException(pluginName);
            //    }
            //
            //    var current = _loaders[pluginName];
            //
            //    if (current == null)
            //    {
            //        throw new UnknownDependencyException(pluginName);
            //    }
            //
            //}
            //
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

            return loader.Plugin;
        }

        public string GetAssemblyNamespace(Assembly asm)
        {
            var ns = @"";
            foreach (var tp in asm.Modules.First().GetTypes()) //Iterate all types within the specified assembly.
                if (tp.Namespace != null && (ns.Length == 0 || tp.Namespace.Length < ns.Length)) //Check whether that's the shortest so far.
                    ns = tp.Namespace; //If it's, set it to the variable.
            return ns; //Now it is the namespace of the assembly.
        }

        string GetResourceFile(string assemblyPath, string fileName)
        {
            // TODO: search for file in every sub namespace
            var assembly = Assembly.LoadFrom(assemblyPath);
            var nameSpace = GetAssemblyNamespace(assembly);
            var resourceName = nameSpace + "." + fileName;
            using var stream = assembly.GetManifestResourceStream(resourceName);
            using var reader = new StreamReader(stream ?? throw new InvalidOperationException());

            var resource = reader.ReadToEnd();

            return resource;
        }

        public PluginDescriptionFile GetPluginDescription(FileInfo file)
        {
            Validate.NotNull(file, "File cannot be null");

            var content = GetResourceFile(file.FullName, "plugin.yml");

            return PluginDescriptionFile.Load(content);

            //JarFile jar = null;
            //InputStream stream = null;
            //
            //try {
            //    jar = new JarFile(file);
            //    JarEntry entry = jar.getJarEntry("plugin.yml");
            //
            //    if ((entry == null)) {
            //        throw new InvalidDescriptionException(new FileNotFoundException("Jar does not contain plugin.yml"));
            //    }
            //
            //    stream = jar.getInputStream(entry);
            //    return new PluginDescriptionFile(stream);
            //}
            //catch (IOException ex) {
            //    throw new InvalidDescriptionException(ex);
            //}
            //catch (YAMLException ex) {
            //    throw new InvalidDescriptionException(ex);
            //}
            //finally {
            //    if (jar != null) {
            //        try {
            //            jar.close();
            //        }
            //        catch (IOException e) {
            //        
            //        }
            //    
            //    }
            //
            //    if (stream != null) {
            //        try {
            //            stream.close();
            //        }
            //        catch (IOException e) {
            //        
            //        }
            //    
            //    }
            //
            //}

        }

        public object[] GetPluginFileFilters()
        {
            //return this.fileFilters.clone();
            return null;
        }



        //Class GetClassByName(String name) {
        //    Class cachedClass = this.classes.get(name);
        //    if ((cachedClass != null)) {
        //        return cachedClass;
        //    }
        //    else {
        //        foreach (String current in this.loaders.keySet()) {
        //            PluginClassLoader loader = this.loaders.get(current);
        //            try {
        //                cachedClass = loader.findClass(name, false);
        //            }
        //            catch (ClassNotFoundException cnfe) {
        //                
        //            }
        //            
        //            if ((cachedClass != null)) {
        //                return cachedClass;
        //            }
        //            
        //        }
        //        
        //    }
        //    
        //    return null;
        //}

        //void setClass(String name, Class clazz) {
        //    if (!this.classes.containsKey(name)) {
        //        this.classes.put(name, clazz);
        //        if (ConfigurationSerializable.class.isAssignableFrom(clazz)) {
        //            Class<ConfigurationSerializable> serializable = clazz.asSubclass(ConfigurationSerializable.class);
        //            ConfigurationSerialization.registerClass(serializable);
        //        }
        //        
        //    }
        //    
        //}
        //
        //private void removeClass(String name) {
        //    Class clazz = this.classes.remove(name);
        //    try {
        //        if (((clazz != null) 
        //                    && ConfigurationSerializable.class.isAssignableFrom(clazz))) {
        //            Class<ConfigurationSerializable> serializable = clazz.asSubclass(ConfigurationSerializable.class);
        //            ConfigurationSerialization.unregisterClass(serializable);
        //        }
        //        
        //    }
        //    catch (NullPointerException ex) {
        //        //  Boggle!
        //        //  (Native methods throwing NPEs is not fun when you can't stop it before-hand)
        //    }
        //    
        //}

        public object /*Map<Class<Event>, Set<RegisteredListener>>*/ CreateRegisteredListeners(IListener listener, IPlugin csharpPlugin)
        {
            //Validate.notNull(plugin, "Plugin can not be null");
            //Validate.notNull(listener, "IListener can not be null");
            //bool useTimings = this.server.GetPluginManager().useTimings();
            //Map<Class<Event>, Set<RegisteredListener>> ret = new HashMap<Class<Event>, Set<RegisteredListener>>();
            //Set<Method> methods;
            //try {
            //    Method[] publicMethods = listener.GetClass().getMethods();
            //    methods = new HashSet<Method>(publicMethods.length, Float.MAX_VALUE);
            //    foreach (Method method in publicMethods) {
            //        methods.add(method);
            //    }
            //    
            //    foreach (Method method in listener.GetClass().getDeclaredMethods()) {
            //        methods.add(method);
            //    }
            //    
            //}
            //catch (NoClassDefFoundError e) {
            //    plugin.GetLogger().severe(("Plugin " 
            //                    + (plugin.GetDescription().GetFullName() + (" has failed to register events for " 
            //                    + (listener.GetClass() + (" because " 
            //                    + (e.getMessage() + " does not exist.")))))));
            //    return ret;
            //}
            //
            //foreach (Method method in methods) {
            //    EventHandler eh = method.getAnnotation(EventHandler.class);
            //    if ((eh == null)) {
            //        // TODO: Warning!!! continue If
            //    }
            //    
            //    Class checkClass;
            //    if (((method.getParameterTypes().length != 1) 
            //                || !Event.class.isAssignableFrom(checkClass=method.getParameterTypes(Unknown[0Unknown))) {
            //        plugin.GetLogger().severe((plugin.GetDescription().GetFullName() + " attempted to register an invalid EventHandler method signature \\\"\" + method.toGenericString() + "), (" in " + listener.GetClass()));
            //        // TODO: Warning!!! continue If
            //    }
            //    
            //    Class<Event> eventClass = checkClass.asSubclass(Event.class);
            //    method.setAccessible(true);
            //    Set<RegisteredListener> eventSet = ret.get(eventClass);
            //    if ((eventSet == null)) {
            //        eventSet = new HashSet<RegisteredListener>();
            //        ret.put(eventClass, eventSet);
            //    }
            //    
            //    for (Class clazz = eventClass; Event.class.isAssignableFrom(clazz); clazz = clazz.getSuperclass()) {
            //        //  This loop checks for extending deprecated events
            //        if ((clazz.getAnnotation(Deprecated.class) != null)) {
            //            Warning warning = clazz.getAnnotation(Warning.class);
            //            WarningState warningState = this.server.getWarningState();
            //            if (!warningState.printFor(warning)) {
            //                break;
            //            }
            //            
            //            plugin.GetLogger().log(Level.WARNING, String.format(("\\\"%s\\\" has registered a listener for %s on method \\\"%s\\\", but the event is Deprecated." + " \\\"%s\\\"; please notify the authors %s."), plugin.GetDescription().GetFullName(), clazz.GetName(), method.toGenericString(), ((warning != null) 
            //                                && (warning.reason().length() != 0))));
            //            // TODO: Warning!!!, inline IF is not supported ?
            //            break;
            //        }
            //        
            //    }
            //    
            //    EventExecutor executor = new EventExecutor();
            //    if (useTimings) {
            //        eventSet.add(new TimedRegisteredListener(listener, executor, eh.priority(), plugin, eh.ignoreCancelled()));
            //    }
            //    else {
            //        eventSet.add(new RegisteredListener(listener, executor, eh.priority(), plugin, eh.ignoreCancelled()));
            //    }
            //    
            //}
            //
            //return ret;

            return null;
        }

        public void EnablePlugin(IPlugin plugin)
        {
            Validate.IsTrue(plugin is CsharpPlugin, "Plugin is not associated with this PluginLoader");
            if (plugin.IsEnabled()) return;

            plugin.GetLogger().Info("Enabling " + plugin.GetDescription().GetFullName());
            var jPlugin = (CsharpPlugin)(plugin);
            var pluginName = jPlugin.GetDescription().GetName();

            //if (!_loaders.ContainsKey(pluginName))
            //{
            //    //loaders.Add(pluginName, ((PluginClassLoader)(jPlugin.GetClassLoader())));
            //}

            try
            {
                jPlugin.SetEnabled(true);
            }
            catch (Exception)
            {
                Handler.GetLogger().Error($"Error occurred while enabling {plugin.GetDescription().GetFullName()} (Is it up to date?)");
            }

            //  Perhaps abort here, rather than continue going, but as it stands,
            //  an abort is not possible the way it's currently written
            //server.GetPluginManager().CallEvent(new PluginEnableEvent(plugin));
        }

        public void DisablePlugin(IPlugin plugin)
        {
            Validate.IsTrue(plugin is CsharpPlugin, "Plugin is not associated with this PluginLoader");
            if (!plugin.IsEnabled()) return;

            string message = $"Disabling {plugin.GetDescription().GetFullName()}";
            plugin.GetLogger().Info(message);
            //server.GetPluginManager().CallEvent(new PluginDisableEvent(plugin));
            var jPlugin = ((CsharpPlugin)(plugin));

            //ClassLoader cloader = jPlugin.GetClassLoader();

            try
            {
                jPlugin.SetEnabled(false);
            }
            catch (Exception)
            {
                Handler.GetLogger().Error($"Error occurred while disabling {plugin.GetDescription().GetFullName()} (Is it up to date?)");
            }

            //_loaders.Remove(jPlugin.GetDescription().GetName());

            //if (cloader is PluginClassLoader) {
            //    PluginClassLoader loader = ((PluginClassLoader)(cloader));
            //    List<string> names = loader.GetClasses();
            //    foreach (var name in names) {
            //        //this.RemoveClass(name);
            //    }
            //}
        }
    }
}