///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Bukkit'
// https://github.com/Bukkit/Bukkit
// which is released under GNU General Public License v3.0.
// https://www.gnu.org/licenses/gpl-3.0.en.html
// Copyright 2021 Bukkit Team
///////////////////////////////////////////////////////////////////

using API.Plugins;

namespace SamplePlugin
{
    // ReSharper disable once UnusedMember.Global
    public class Main : Plugin
    {
        /// <summary>
        /// A parameterless constructor is mandatory for the plugin to work
        /// </summary>
        // ReSharper disable once EmptyConstructor
        public Main() { }

        public override void OnDisable()
        {
            // TODO: Place any custom disable code here

            // NOTE: All registered events are automatically unregistered when a plugin is disabled

            // EXAMPLE: Custom code, here we just output some info so we can check all is well
            GetLogger().Info("Goodbye world!");
        }

        public override void OnEnable()
        {
            // TODO: Place any custom enable code here including the registration of any events

            // Register our events
            var pm = GetServer().GetPluginManager();
            pm.RegisterListeners(new Listener(this), this, true);

            // TODO: Add the ability
            //// Register our commands
            //getCommand("pos").setExecutor(new SamplePosCommand());
            //getCommand("debug").setExecutor(new SampleDebugCommand(this));

            // EXAMPLE: Custom code, here we just output some info so we can check all is well
            var pdfFile = GetDescription();
            GetLogger().Info($"{pdfFile.GetName()} version {pdfFile.GetVersion()} is enabled!");
        }
    }
}
