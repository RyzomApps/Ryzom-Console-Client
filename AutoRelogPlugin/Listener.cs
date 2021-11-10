using System;
using API.Plugins;

namespace AutoRelogPlugin
{
    /// <summary>
    /// Handle events for all Player related events
    /// </summary>
    /// <author>bierdosenhalter</author>
    internal class Listener : ListenerBase
    {
        private readonly Main _plugin;
        private DateTime _timeStart;

        public Listener(Main plugin)
        {
            _plugin = plugin;
        }

        public override void OnInitialize()
        {
            _plugin.GetLogger().Info("Automaton 'AutoRelog' initialized.");
            _timeStart = DateTime.Now;
        }

        public override void OnUpdate()
        {
            if ((DateTime.Now - _timeStart).TotalSeconds <= 30/*Config.ClientConfig.AutoRelogSeconds*/) return;

            // To avoid restart cancellation
            _timeStart = _timeStart.AddSeconds(60);

            _plugin.GetLogger().Info("[AutoRelog] Restarting client...");
            var responseMessage = "";
            _plugin.GetServer().PerformInternalCommand("Quit", ref responseMessage);
        }
    }
}
