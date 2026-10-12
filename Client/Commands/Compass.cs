///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////
using API;
using API.Commands;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Client.Commands
{
    /// <summary>
    /// Display the compass landmarks the server has sent (mission targets
    /// and other JOURNAL:ADD_COMPASS entries), sorted by distance.
    /// </summary>
    public class Compass : CommandBase
    {
        public override CommandCategory CmdCategory => CommandCategory.Info;
        public override string CmdName => "Compass";

        public override string CmdUsage => "[clear]";

        public override string CmdDesc => "List the compass landmarks sent by the server (mission targets etc.), sorted by distance from you. 'Compass clear' empties the local list.";

        public override bool Run(IClient handler, string command, out string responseMsg, Dictionary<string, object> localVars)
        {
            responseMsg = "";
            if (handler is not RyzomClient ryzomClient)
                throw new Exception("Command handler is not a Ryzom client.");

            GetArgs(command);

            var args = command.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (args.Length == 1 && args[0].Equals("clear", StringComparison.OrdinalIgnoreCase))
            {
                ryzomClient.GetCompassManager().Clear();
                responseMsg = "Compass list cleared.";
                return true;
            }

            var entries = ryzomClient.GetCompassManager().GetEntries();

            if (entries.Count == 0)
            {
                responseMsg = "No compass landmarks received. Pick up a mission with an on-map target (BOTCHAT:START_CHOOSE_MISSION -> accept) and try again.";
                return true;
            }

            var user = ryzomClient?.GetApiNetworkManager()?.GetApiEntityManager()?.GetApiUserEntity();
            if (user == null)
            {
                responseMsg = "User entity missing.";
                return false;
            }

            var networkManager = ryzomClient.GetNetworkManager();
            var stringManager = ryzomClient.GetStringManager();

            var sorted = entries
                .Select(e => new { Entry = e, Distance = Math.Sqrt(Math.Pow(e.X / 1000.0 - user.Pos.X, 2) + Math.Pow(e.Y / 1000.0 - user.Pos.Y, 2)) })
                .OrderBy(e => e.Distance)
                .ToList();

            var lines = new List<string>
            {
                $"{sorted.Count} compass landmark(s):",
                $"{"Text",-42} {"X",-9} {"Y",-9} {"Dist",-9} Bearing"
            };

            foreach (var item in sorted)
            {
                var e = item.Entry;

                var title = StringWait.WaitString(stringManager, e.TextId, networkManager);

                if (e.IsBot)
                    title += $" [bot {e.BotCompressedIndex}]";

                // Bearing: 0 deg = north (+Y), 90 deg = east (+X)
                var dx = e.X / 1000.0 - user.Pos.X;
                var dy = e.Y / 1000.0 - user.Pos.Y;
                var bearing = (Math.Atan2(dx, dy) * 180.0 / Math.PI + 360.0) % 360.0;

                lines.Add($"{title,-42} {e.X / 1000.0,-9:0.0} {e.Y / 1000.0,-9:0.0} {item.Distance,-9:0} {bearing,6:0}deg");
            }

            responseMsg = string.Join("\n", lines);
            return true;
        }
    }
}
