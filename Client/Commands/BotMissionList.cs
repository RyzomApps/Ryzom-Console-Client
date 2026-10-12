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
            var sb = new StringBuilder();
            sb.AppendLine($"Mission page {(int)botChatManager.MissionPageId}{(botChatManager.MissionHasNextPage ? "" : " (last page)")}");

            foreach (var entry in list)
            {
                var title = StringWait.WaitString(stringManager, entry.TextId, networkManager);
                sb.AppendLine($"{(int)entry.Index,-5} {Trunc(title, 60),-60} [prereq {(int)entry.PrereqState}]");
            }

            responseMsg = sb.ToString().TrimEnd();
            return true;
        }

        private static string Trunc(string s, int len)
        {
            if (s.Length <= len)
                return s;
            return s.Substring(0, len - 3) + "...";
        }
    }
}
