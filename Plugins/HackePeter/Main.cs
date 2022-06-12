using API.Plugins;

namespace HackePeter
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

            // Register listener
            var pm = GetClient().GetPluginManager();
            pm.RegisterListeners(listener, this, true);
        }
    }
}
