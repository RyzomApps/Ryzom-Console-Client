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
using System.Text;

namespace Client.Commands
{
    /// <summary>
    /// List the missions currently in the character's journal (SERVER:MISSIONS, 15 slots).
    /// Mirrors the official client's mission journal display.
    /// </summary>
    public class Missions : CommandBase
    {
        public override CommandCategory CmdCategory => CommandCategory.Info;
        public override string CmdName => "Missions";

        public override string CmdUsage => string.Empty;

        public override string CmdDesc => "List your accepted missions from the character journal (SERVER:MISSIONS).";

        public override System.Collections.Generic.IEnumerable<string> GetCmdAliases()
        {
            yield return "BotMissionJournal";
        }

        private const int MaxMissions = 15;
        private const int MaxGoals = 20;
        private const int MaxTargets = 8;

        public override bool Run(IClient handler, string command, out string responseMsg, Dictionary<string, object> localVars)
        {
            responseMsg = "";
            if (handler is not RyzomClient ryzomClient)
                throw new Exception("Command handler is not a Ryzom client.");

            var stringManager = ryzomClient.GetStringManager();
            var networkManager = ryzomClient.GetNetworkManager();

            long Safe(string leaf) => MissionDb.SafeProp(ryzomClient, leaf, -1);

            string Resolve(uint textId)
            {
                if (textId == 0 || textId == uint.MaxValue)
                    return string.Empty;
                return StringWait.WaitString(stringManager, textId, networkManager);
            }

            var lines = new List<string>();
            var active = 0;

            for (var slot = 0; slot < MaxMissions; slot++)
            {
                var type = Safe($"MISSIONS:{slot}:TYPE");

                // TYPE 0 = empty slot (see game_share/mission_desc.h TMissionType)
                if (type <= 0)
                    continue;

                active++;

                var title = Resolve((uint)Safe($"MISSIONS:{slot}:TITLE"));
                var detail = Resolve((uint)Safe($"MISSIONS:{slot}:DETAIL_TEXT"));
                var finished = Safe($"MISSIONS:{slot}:FINISHED");
                var sleep = Safe($"MISSIONS:{slot}:SLEEP") == 1;

                var state = finished switch
                {
                    0 => "active",
                    1 => "success",
                    2 => "failed",
                    _ => $"state {finished}"
                };
                if (sleep)
                    state += ", sleeping (queue)";

                lines.Add($"[{slot}] {title}  ({state})");
                if (!string.IsNullOrEmpty(detail))
                    lines.Add($"    {detail.Replace("\n", " | ")}");

                // Goals
                for (var goal = 0; goal < MaxGoals; goal++)
                {
                    var text = Resolve((uint)Safe($"MISSIONS:{slot}:GOALS:{goal}:TEXT"));
                    if (string.IsNullOrEmpty(text) || text.StartsWith("TEXT ", StringComparison.Ordinal))
                        continue;
                    lines.Add($"    Goal: {text}");
                }

                // Targets (step targets with coordinates, like compass entries)
                for (var target = 0; target < MaxTargets; target++)
                {
                    // counted branch: node names are TARGET0..TARGET7 (no separator)
                    var prefix = $"MISSIONS:{slot}:TARGET{target}";
                    var x = Safe($"{prefix}:X");
                    var y = Safe($"{prefix}:Y");
                    if (x <= 0 && y <= 0)
                        continue;

                    var targetTitle = Resolve((uint)Safe($"{prefix}:TITLE"));
                    lines.Add($"    Target: {targetTitle} @ {x / 1000.0:0.0}, {y / 1000.0:0.0}");
                }
            }

            if (lines.Count == 0)
            {
                responseMsg = "No missions in the journal.";
                return true;
            }

            responseMsg = $"{active} mission(s):\n" + string.Join("\n", lines);
            return true;
        }
    }

    /// <summary>
    /// Shared null-safe database read for mission-related commands.
    /// </summary>
    internal static class MissionDb
    {
        public static long SafeProp(RyzomClient ryzomClient, string name, long fallback)
        {
            var node = ryzomClient.GetDatabaseManager().GetServerNode(name, false);
            return node?.GetValue64() ?? fallback;
        }
    }
}
