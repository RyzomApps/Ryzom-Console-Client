///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////
using API;
using API.BotChat;
using API.Commands;
using Client.BotChat;
using System;
using System.Collections.Generic;

namespace Client.Commands
{
    /// <summary>
    /// Ask the server for the next page of the trade list.
    /// </summary>
    public class BotTradeNext : CommandBase
    {

        public override CommandCategory CmdCategory => CommandCategory.Npc;
        public override string CmdName => "BotTradeNext";

        public override string CmdUsage => "";

        public override string CmdDesc => "Request the next trade list page from the server (then use 'BotTradeList' to display it)";

        public override bool Run(IClient handler, string command, out string responseMsg, Dictionary<string, object> localVars)
        {
            responseMsg = "";
            if (handler is not RyzomClient ryzomClient)
                throw new Exception("Command handler is not a Ryzom client.");

            var botChatManager = ryzomClient.GetBotChatManager();
            if (botChatManager.CurrentSessionId == 0)
            {
                responseMsg = "No trade session running. Use 'BotTradeStart <type>' first.";
                return false;
            }

            if (!botChatManager.SendNextTradePage())
            {
                responseMsg = "Failed to send the next page request.";
                return false;
            }

            responseMsg = $"Next page requested (current page: {botChatManager.PageId}). Use 'BotTradeList' to display it.";
            return true;
        }
    }
}
