using API;
using API.Chat;
using API.Commands;
using Client.Helper;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Client.Commands
{
    /// <summary>
    /// TPS refers to the ticks per second. If the TPS is 10, ryzom's server is running flawlessly.
    /// </summary>
    public class TicksPerSecond : CommandBase
    {
        public override string CmdName => "tps";

        public override string CmdUsage => "";

        public override string CmdDesc => "Gets the current ticks per second for the server";

        public override bool Run(IClient handler, string command, out string responseMsg, Dictionary<string, object> localVars)
        {
            responseMsg = "";
            var tps = handler.GetApiNetworkManager().GetTps();
            var tpsAvg = new string[tps.Length];

            for (var i = 0; i < tps.Length; i++)
                tpsAvg[i] = Format(tps[i]);

            responseMsg = $"{ChatColor.GOLD}TPS from last 1m, 5m, 15m: {string.Join(", ", tpsAvg)}";
            return false;
        }

        private static string Format(double tps)
        {
            var percentage = tps / RollingAverage.GameTps;

            return (percentage > 0.9 ? ChatColor.GREEN : percentage > 0.8 ? ChatColor.YELLOW : ChatColor.RED) +
                   string.Format(CultureInfo.InvariantCulture, "{0:0.00}", Math.Round(tps * 100d) / 100d);
        }
    }
}