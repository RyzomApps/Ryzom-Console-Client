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

            RelogSeconds = GetConfig().GetInt("relogSeconds");
            if (RelogSeconds < 60) RelogSeconds = 60;

            GetLogger().Info($"After {RelogSeconds} seconds, the client will shut down automatically. Please use a restart script to ensure that the client continues to operate.");

            // Register listener
            var pm = GetClient().GetPluginManager();
            pm.RegisterListeners(new Listener(this), this, true);
        }
    }
}
