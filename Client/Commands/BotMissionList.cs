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
using System.Text;

namespace Client.Commands
{
    /// <summary>
    /// List the missions currently offered by the bot (choose page,
    /// SERVER:CHOOSE_MISSIONS branch).
    /// </summary>
    public class BotMissionList : CommandBase
    {
        public override CommandCategory CmdCategory => CommandCategory.Npc;
        public override string CmdName => "BotMissionList";

        public override string CmdUsage => string.Empty;

        public override string CmdDesc => "List the missions offered by the currently targeted NPC (choose page).";

        // Result may follow asynchronously through the logger when mission titles
        // are not cached yet (RunAsyncTask); immediate when all strings are cached.
        public override bool CmdAsync => true;

        public override System.Collections.Generic.IEnumerable<string> GetCmdAliases()
        {
            yield break;
        }

        public override bool Run(IClient handler, string command, out string responseMsg, Dictionary<string, object> localVars)
        {
            responseMsg = "";
            if (handler is not RyzomClient ryzomClient)
                throw new Exception("Command handler is not a Ryzom client.");

            var botChatManager = ryzomClient.GetBotChatManager();

            if (botChatManager.CurrentMissionSessionId == 0)
            {
                responseMsg = "No mission session running. Use 'BotMissionOpen' first.";
                return false;
            }

            var list = botChatManager.GetMissionList();
            if (list.Count == 0)
            {
                responseMsg = "The bot offers no missions on this page.";
                return true;
            }

            var stringManager = ryzomClient.GetStringManager();
            var networkManager = ryzomClient.GetNetworkManager();

            // resolve all titles in one batched request round; non-blocking:
            // if not everything is cached, the result is delivered later
            // through the logger like a normal command result
            var ids = new List<uint>();
            foreach (var entry in list)
                ids.Add(entry.TextId);

            var waiter = StringWait.WaitAllAsync(stringManager, ids, networkManager);

            if (waiter.Completed)
            {
                responseMsg = Render(resolved: waiter.Results);
                return true;
            }

            RunAsyncTask(handler, () => (true, Render(waiter.Wait())));
            responseMsg = $"Reading the bot's mission list - titles are being requested, result follows...";
            return true;

            // Render the mission page from resolved strings (closure over the
            // Run locals; runs on the main thread synchronously, or on the
            // async task after resolution)
            string Render(Dictionary<uint, string> resolved)
            {
            var sb = new StringBuilder();
            sb.AppendLine($"Mission page {(int)botChatManager.MissionPageId}{(botChatManager.MissionHasNextPage ? "" : " (last page)")}");

            foreach (var entry in list)
            {
                var title = resolved.TryGetValue(entry.TextId, out var value) && !string.IsNullOrEmpty(value) ? value : $"TEXT {entry.TextId}";
                sb.AppendLine($"{(int)entry.Index,-5} {Trunc(title, 60),-60} [pre: {PrereqName(entry.PrereqState)}]");
            }

            return sb.ToString().TrimEnd();
            }
        }

        // MISSION_DESC::TPreReqState (mission_desc.h:156)
        private static string PrereqName(byte state) => state switch
        {
            0 => "ok",       // PreReqSuccess
            1 => "done",     // PreReqFailAlreadyDone
            2 => "fail",     // PreReqFail
            3 => "running",  // PreReqFailRunning
            _ => $"unknown {state}"
        };

        private static string Trunc(string s, int len)
        {
            if (s.Length <= len)
                return s;
            return s.Substring(0, len - 3) + "...";
        }
    }
}
