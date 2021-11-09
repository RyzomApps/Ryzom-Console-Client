///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Bukkit'
// https://github.com/Bukkit/Bukkit
// which is released under GNU General Public License v3.0.
// https://www.gnu.org/licenses/gpl-3.0.en.html
// Copyright 2021 Bukkit Team
///////////////////////////////////////////////////////////////////

using System.IO;

namespace API.Plugins.Interfaces
{
    public interface IPluginLoader {
    
        IClient Handler { get; set; }

        IPlugin LoadPlugin(FileInfo file);
    
        PluginDescriptionFile GetPluginDescription(FileInfo file);
    
        object[] GetPluginFileFilters();
    
        object CreateRegisteredListeners(IListener listener, IPlugin plugin);
    
        void EnablePlugin(IPlugin plugin);
    
        void DisablePlugin(IPlugin plugin);
    }
}