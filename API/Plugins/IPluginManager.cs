
using System;
using System.IO;
using API.Plugins.Interfaces;

namespace API.Plugins
{
    public interface IPluginManager {
    
        void registerInterface(IPluginLoader loader);
    
        IPlugin getPlugin(string name);
    
        IPlugin[] getPlugins();
    
        bool isPluginEnabled(string name);
    
        bool isPluginEnabled(IPlugin plugin);
    
        IPlugin loadPlugin(FileInfo file);
    
       IPlugin[] loadPlugins(DirectoryInfo directory);
    
        void disablePlugins();
    
        void clearPlugins();
    
        //void callEvent(Event event);
        //
        //void registerEvents(Listener listener, Plugin plugin);
        //
        //void registerEvent(Class<Event> event, Listener listener, EventPriority priority, EventExecutor executor, Plugin plugin);
        //
        //void registerEvent(Class<Event> event, Listener listener, EventPriority priority, EventExecutor executor, Plugin plugin, bool ignoreCancelled);
    
        void enablePlugin(IPlugin plugin);
    
        void disablePlugin(IPlugin plugin);
    
        //Permission getPermission(string name);
        //
        //void addPermission(Permission perm);
        //
        //void removePermission(Permission perm);
        //
        //void removePermission(string name);
        //
        //Set<Permission> getDefaultPermissions(bool op);
        //
        //void recalculatePermissionDefaults(Permission perm);
        //
        //void subscribeToPermission(string permission, Permissible permissible);
        //
        //void unsubscribeFromPermission(string permission, Permissible permissible);
        //
        //Set<Permissible> getPermissionSubscriptions(string permission);
        //
        //void subscribeToDefaultPerms(bool op, Permissible permissible);
        //
        //void unsubscribeFromDefaultPerms(bool op, Permissible permissible);
        //
        //Set<Permissible> getDefaultPermSubscriptions(bool op);
        //
        //Set<Permission> getPermissions();
    
        bool useTimings();
    }
}