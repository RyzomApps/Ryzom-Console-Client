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
    /// Ask the server for the next page of the current mission list.
    /// </summary>
    public class BotMissionNext : CommandBase
    {

        public override CommandCategory CmdCategory => CommandCategory.Npc;
        public override string CmdName => "BotMissionNext";

        public override string CmdUsage => "";

        public override string CmdDesc => "Ask the server for the next page of the current mission list";

        public override bool Run(IClient handler, string command, out string responseMsg, Dictionary<string, object> localVars)
        {
            responseMsg = "";
            if (handler is not RyzomClient ryzomClient)
                throw new Exception("Command handler is not a Ryzom client.");

            GetArgs(command); // no arguments

            var botChatManager = ryzomClient.GetBotChatManager();

            if (botChatManager.CurrentMissionSessionId == 0)
            {
                responseMsg = "No mission session running. Use 'BotMissionStart' first.";
                return false;
            }

            var sent = botChatManager.SendNextMissionPage();
            responseMsg = sent
                ? "Next page requested. Use 'BotMissionList' to display it once it has arrived."
                : "Failed to send the next-page request.";
            return sent;
        }
    }
}
