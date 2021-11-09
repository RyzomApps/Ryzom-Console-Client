using System;
using RCC.Automata.Internal;

namespace RCC.Automata
{
    class AutoRelog : AutomatonBase
    {
        private DateTime _timeStart;

        public override void OnInitialize()
        {
            Handler.GetLogger().Info("Automaton 'AutoRelog' initialized.");
            _timeStart = DateTime.Now;
        }

        public override void OnUpdate()
        {
            if ((DateTime.Now - _timeStart).TotalSeconds <= Config.ClientConfig.AutoRelogSeconds) return;

            // To avoid restart cancellation
            _timeStart = _timeStart.AddSeconds(60);

            Handler.Log.Info("[AutoRelog] Restarting client...");
            var responseMessage = "";
            Handler.PerformInternalCommand("Quit", ref responseMessage);
        }
    }
}