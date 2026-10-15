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
using Client.Strings;

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

            var textIds = new List<uint>();
            var active = 0;
            void Collect(uint id) { if (id != 0 && id != uint.MaxValue) textIds.Add(id); }

            for (var slot = 0; slot < MaxMissions; slot++)
            {
                // A slot is occupied when TITLE != 0; TYPE is the mission type
                // (TMissionType: Solo=0, Group=1, Guild=2), so solo missions
                // legitimately have TYPE 0. Empty slots have TITLE 0.
                if (Safe($"MISSIONS:{slot}:TITLE") == 0)
                    continue;

                active++;

                Collect((uint)Safe($"MISSIONS:{slot}:TITLE"));
                Collect((uint)Safe($"MISSIONS:{slot}:DETAIL_TEXT"));

                // Goals
                for (var goal = 0; goal < MaxGoals; goal++)
                    Collect((uint)Safe($"MISSIONS:{slot}:GOALS:{goal}:TEXT"));

                // Targets (step targets with coordinates, like compass entries)
                for (var target = 0; target < MaxTargets; target++)
                {
                    // counted branch: node names are TARGET0..TARGET7 (no separator)
                    var prefix = $"MISSIONS:{slot}:TARGET{target}";
                    var x = Safe($"{prefix}:X");
                    var y = Safe($"{prefix}:Y");
                    if (x > 0 || y > 0)
                        Collect((uint)Safe($"{prefix}:TITLE"));
                }
            }

            if (active == 0)
            {
                responseMsg = "No missions in the journal.";
                return true;
            }

            // resolve all collected ids in one batched request round;
            // non-blocking: if not everything is cached, the result is
            // delivered later through the logger like a normal command result
            var waiter = StringWait.WaitAllAsync(stringManager, textIds, networkManager);

            if (waiter.Completed)
            {
                responseMsg = Render(waiter.Results);
                return true;
            }

            RunAsyncTask(handler, () => (true, Render(waiter.Wait())));
            responseMsg = $"Reading {active} mission(s) - strings are being requested, result follows...";
            return true;

        // Render the journal listing from resolved strings (closure over the
        // Run locals; runs on the main thread synchronously, or on the async
        // task after resolution)
        string Render(Dictionary<uint, string> resolved)
        {
            string Text(uint id) =>
                id == 0 || id == uint.MaxValue ? string.Empty :
                resolved.TryGetValue(id, out var value) && !string.IsNullOrEmpty(value) ? value : $"TEXT {id}";

            var lines = new List<string>();

            for (var slot = 0; slot < MaxMissions; slot++)
            {
                if (Safe($"MISSIONS:{slot}:TITLE") == 0)
                    continue;

                var title = Text((uint)Safe($"MISSIONS:{slot}:TITLE"));
                var detail = Text((uint)Safe($"MISSIONS:{slot}:DETAIL_TEXT"));
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
                    var text = Text((uint)Safe($"MISSIONS:{slot}:GOALS:{goal}:TEXT"));
                    if (string.IsNullOrEmpty(text) || text.StartsWith("TEXT ", StringComparison.Ordinal))
                        continue;
                    lines.Add($"    Goal: {text}");
                }

                // Targets (step targets with coordinates, like compass entries)
                for (var target = 0; target < MaxTargets; target++)
                {
                    var prefix = $"MISSIONS:{slot}:TARGET{target}";
                    var x = Safe($"{prefix}:X");
                    var y = Safe($"{prefix}:Y");
                    if (x <= 0 && y <= 0)
                        continue;

                    var targetTitle = Text((uint)Safe($"{prefix}:TITLE"));
                    lines.Add($"    Target: {targetTitle} @ {x / 1000.0:0.0}, {y / 1000.0:0.0}");
                }
            }

            return $"{active} mission(s):\n" + string.Join("\n", lines);
        }
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
