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
            _plugin.GetLogger().Info("§cT§6h§ee §9e§bx§5a§cm§6p§el§ae §bp§5l§cu§6g§ei§an §bw§5a§cs §el§ao§9a§bd§5e§cd §es§au§9c§bc§5e§cs§6s§ef§au§9l§bl§5y§c!");
        }

        public override void OnInitialize()
        {
            _plugin.GetLogger().Info("Initialized!");
        }
    }
}
