///////////////////////////////////////////////////////////////////
// This file contains modified code from 'SpigotMC'
// https://www.spigotmc.org/
// which is released under GPL-3.0 License.
// https://www.gnu.org/licenses/gpl-3.0.en.html
// Copyright 2020 SpigotMC
///////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Globalization;
using API;
using API.Chat;
using API.Commands;
using Client.Helper;

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

        public override string Run(IClient handler, string command, Dictionary<string, object> localVars)
        {
            var tps = handler.GetApiNetworkManager().GetTps();
            var tpsAvg = new string[tps.Length];

            for (var i = 0; i < tps.Length; i++) tpsAvg[i] = Format(tps[i]);

            return ChatColor.GOLD + "TPS from last 1m, 5m, 15m: " + string.Join(", ", tpsAvg);
        }

        private static string Format(double tps)
        {
            var percentage = tps / RollingAverage.GameTps;

            return (percentage > 0.9 ? ChatColor.GREEN :
                       percentage > 0.8 ? ChatColor.YELLOW : ChatColor.RED) /*+ (percentage > 1.0 ? "*" : "")*/
                   + string.Format(CultureInfo.InvariantCulture, "{0:0.00}", /*Math.Min(*/
                       Math.Round(tps * 100.0) / 100.0 /*, RollingAverage.GameTps)*/);
        }
    }
}