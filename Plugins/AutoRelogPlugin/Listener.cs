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
            _timeStart = DateTime.Now;
            _plugin.GetLogger().Info("Initialized at " + _timeStart.ToShortTimeString());
        }

        public override void OnUpdate()
        {
            if ((DateTime.Now - _timeStart).TotalSeconds <= _plugin.RelogSeconds) return;

            // To avoid restart cancellation
            _timeStart = _timeStart.AddSeconds(60);

            _plugin.GetLogger().Info("Restarting client after " + _plugin.RelogSeconds + " seconds...");
            var responseMessage = "";
            _plugin.GetServer().PerformInternalCommand("Quit", ref responseMessage);
        }
    }
}
