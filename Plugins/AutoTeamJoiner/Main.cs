using API.Plugins;

namespace AutoTeamJoiner
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
            // Register listener
            var pm = GetClient().GetPluginManager();
            pm.RegisterListeners(new Listener(this), this, true);
        }
    }
}
