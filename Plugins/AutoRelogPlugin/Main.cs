using API.Plugins;

namespace AutoRelogPlugin
{
    // ReSharper disable once UnusedMember.Global
    public class Main : Plugin
    {
        public int RelogSeconds { get; private set; }

        /// <summary>
        /// A parameterless constructor is mandatory for the plugin to work
        /// </summary>
        // ReSharper disable once EmptyConstructor
        public Main() { }

        public override void OnEnable()
        {
            // Config
            SaveDefaultConfig();
            ReloadConfig();
            
            // TODO: proper load yml config
            RelogSeconds = GetConfig().GetInt("relogSeconds");
            if (RelogSeconds < 60) RelogSeconds = 60;

            // Register listener
            var pm = GetServer().GetPluginManager();
            pm.RegisterListeners(new Listener(this), this, true);
        }
    }
}
