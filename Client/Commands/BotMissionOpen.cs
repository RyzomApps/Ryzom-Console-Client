///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////
using API;
using API.Commands;
using Client.BotChat;
using System;
using System.Collections.Generic;

namespace Client.Commands
{
    /// <summary>
    /// Open the mission menu of the currently targeted bot/NPC.
    /// </summary>
    public class BotMissionOpen : CommandBase
    {

        public override CommandCategory CmdCategory => CommandCategory.Npc;
        public override string CmdName => "BotMissionOpen";

        public override string CmdUsage => "";

        public override string CmdDesc => "Open the mission menu of the currently targeted bot/NPC (choose page follows via BotMissionPage)";

        public override IEnumerable<string> GetCmdAliases()
        {
            yield return "BotMissionStart";
        }

        public override bool Run(IClient handler, string command, out string responseMsg, Dictionary<string, object> localVars)
        {
            responseMsg = "";
            if (handler is not RyzomClient ryzomClient)
                throw new Exception("Command handler is not a Ryzom client.");

            // The server silently drops BOTCHAT messages when the bot is
            // farther than MaxTalkingDist (8 m) - fail early with a hint.
            var proximityError = BotChatProximity.Check(ryzomClient);
            if (proximityError != null)
            {
                responseMsg = proximityError;
                return false;
            }

            GetArgs(command); // no arguments

            var botChatManager = ryzomClient.GetBotChatManager();
            var sent = botChatManager.SendStartChooseMission();

            if (!sent)
            {
                responseMsg = "Failed to send the mission request (no network connection?).";
                return false;
            }

            // The server answers by filling SERVER:CHOOSE_MISSIONS; display
            // the list with 'BotMissionList' once it has arrived.
            responseMsg = $"Mission session started (session: {botChatManager.CurrentMissionSessionId}). " +
                          "Use 'BotMissionList' to display the current page.";
            return true;
        }
    }
}
