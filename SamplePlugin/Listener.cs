using API.Plugins;

namespace SamplePlugin
{
    /// <summary>
    /// Handle events for all Player related events
    /// </summary>
    /// <author>bierdosenhalter</author>
    internal class Listener : ListenerBase
    {
        private readonly Main _plugin;

        public Listener(Main instance)
        {
            _plugin = instance;
            _plugin.GetLogger().Info("§3Listener()");
        }

        public override void OnInitialize()
        {
            _plugin.GetLogger().Info("§3OnInitialize()");
        }
    }
}
