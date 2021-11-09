///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Bukkit'
// https://github.com/Bukkit/Bukkit
// which is released under GNU General Public License v3.0.
// https://www.gnu.org/licenses/gpl-3.0.en.html
// Copyright 2021 Bukkit Team
///////////////////////////////////////////////////////////////////

using System.IO;
using API.Logger;
using API.Plugins;

namespace SamplePlugin
{
    public class SamplePlugin : Plugin
    {
        //private final SamplePlayerListener playerListener = new SamplePlayerListener(this);
        //private final SampleBlockListener blockListener = new SampleBlockListener();
        //private final HashMap<Player, Boolean> debugees = new HashMap<Player, Boolean>();

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
            //PluginManager pm = GetServer().GetPluginManager();
            //pm.registerEvents(playerListener, this);
            //pm.registerEvents(blockListener, this);
            //
            //// Register our commands
            //getCommand("pos").setExecutor(new SamplePosCommand());
            //getCommand("debug").setExecutor(new SampleDebugCommand(this));

            // EXAMPLE: Custom code, here we just output some info so we can check all is well
            var pdfFile = GetDescription();
            GetLogger().Info($"{pdfFile.GetName()} version {pdfFile.GetVersion()} is enabled!");
        }

        //public bool IsDebugging(Player player) {
        //    if (debugees.containsKey(player)) {
        //        return debugees.get(player);
        //    } else {
        //        return false;
        //    }
        //}
        //
        //public void setDebugging(final Player player, final boolean value) {
        //    debugees.put(player, value);
        //}

        public SamplePlugin(PluginLoader loader, PluginDescriptionFile description, DirectoryInfo dataFolder, FileInfo file, ILogger logger) : base(loader, description, dataFolder, file, logger) { }
    }
}
