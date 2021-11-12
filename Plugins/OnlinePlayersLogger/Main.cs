///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Bukkit'
// https://github.com/Bukkit/Bukkit
// which is released under GNU General Public License v3.0.
// https://www.gnu.org/licenses/gpl-3.0.en.html
// Copyright 2021 Bukkit Team
///////////////////////////////////////////////////////////////////

using API.Plugins;

namespace OnlinePlayersLogger
{
    // ReSharper disable once UnusedMember.Global
    public class Main : Plugin
    {
        /// <summary>
        /// A parameterless constructor is mandatory for the plugin to work
        /// </summary>
        // ReSharper disable once EmptyConstructor
        public Main() { }

        public override void OnEnable()
        {
            var listener = new Listener(this);

            // Config
            SaveDefaultConfig();
            ReloadConfig();
            listener.OnlinePlayersApi = GetConfig().GetString("onlinePlayersApi");

            // Register listener
            var pm = GetClient().GetPluginManager();
            pm.RegisterListeners(listener, this, true);
        }
    }
}
