///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Bukkit'
// https://github.com/Bukkit/Bukkit
// which is released under GNU General Public License v3.0.
// https://www.gnu.org/licenses/gpl-3.0.en.html
// Copyright 2021 Bukkit Team
///////////////////////////////////////////////////////////////////

using System.IO;
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
            // Config
            SaveDefaultConfig();
            ReloadConfig();

            // TODO: Place any custom enable code here including the registration of any events

            // Register our events
            var pm = GetClient().GetPluginManager();
            pm.RegisterListeners(new Listener(this), this, true);

            // TODO: Add the ability
            //// Register our commands
            //getCommand("pos").setExecutor(new SamplePosCommand());
            //getCommand("debug").setExecutor(new SampleDebugCommand(this));

            // EXAMPLE: Custom code, here we just output some info so we can check all is well
            var pdfFile = GetDescription();
            GetLogger().Info($"{pdfFile.GetName()} version {pdfFile.GetVersion()} is enabled!");

            // EXAMPLE: creating file/checking important settings
            ConfigExample();
        }

        /// <summary>
        /// When the plugin is enabled you have to possible situations: <br/>
        /// A. Config file doesn't exist <br/>
        /// B. Config file does exist
        /// </summary>
        private void ConfigExample()
        {
            var file = new FileInfo($"{GetDataFolder()}\\config.yml"); //This will get the config file
            
            //This will check if the file exist
            if (!file.Exists)
            {
                //Situation A, File doesn't exist
                GetConfig().AddDefault("Name", "Value"); //adding default settings

                //Save the default settings
                GetConfig().Options().CopyDefaults(true);
                SaveConfig();
            }
            else
            {
                //situation B, Config does exist
                CheckConfig(); //function to check the important settings
                SaveConfig(); //saves the config
                ReloadConfig(); //reloads the config
            }
        }

        /// <summary>
        /// This funcion will check of important settings aren't deleted
        /// </summary>
        public void CheckConfig()
        {
            if (GetConfig().Get("Name") != null) return;

            //if the setting has been deleted it will be null
            GetConfig().Set("Name", "Value"); //reset the setting
            SaveConfig();
            ReloadConfig();
        }
    }
}
