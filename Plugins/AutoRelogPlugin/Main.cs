using API.Plugins;

namespace AutoRelogPlugin
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

            listener.Enabled = GetConfig().GetBool("enabled");
            listener.RelogSeconds = GetConfig().GetInt("relogSeconds");
            if (listener.RelogSeconds < 60) listener.RelogSeconds = 60;

            GetLogger().Info($"After {listener.RelogSeconds} seconds, the client will shut down automatically. Please use a restart script to ensure that the client continues to operate.");

            // Register listener
            var pm = GetClient().GetPluginManager();
            pm.RegisterListeners(new Listener(this), this, true);
        }
    }
}
