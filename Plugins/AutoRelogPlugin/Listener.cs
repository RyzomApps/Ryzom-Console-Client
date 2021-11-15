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
        public int RelogSeconds { get; set; }
        public bool Enabled { get; set; }

        private readonly Main _plugin;
        private DateTime _timeStart;

        public Listener(Main plugin)
        {
            _plugin = plugin;
        }

        public override void OnInitialize()
        {
            _timeStart = DateTime.Now;
        }

        public override void OnUpdate()
        {
            if (!Enabled)
                return;

            if ((DateTime.Now - _timeStart).TotalSeconds <= RelogSeconds) return;

            // To avoid restart cancellation
            _timeStart = _timeStart.AddSeconds(60);

            _plugin.GetLogger().Info($"Restarting client after {RelogSeconds} seconds...");
            var responseMessage = "";
            _plugin.GetClient().PerformInternalCommand("Quit", ref responseMessage);
        }
    }
}
