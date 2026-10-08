///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
using API;
using API.BotChat;
using API.Commands;
using Client.BotChat;
using System;
using System.Collections.Generic;

namespace Client.Commands
{
    /// <summary>
    /// Ask the server to refresh the trade list of the current session.
    /// </summary>
    public class BotTradeRefresh : CommandBase
    {
        public override string CmdName => "BotTradeRefresh";

        public override string CmdUsage => "";

        public override string CmdDesc => "Refresh the trade list of the current trade session (new/removed items, prices)";

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

            var sent = botChatManager.SendRefreshTradeList();
            responseMsg = sent
                ? "Refresh requested. Use 'BotTradeList' to display the updated list."
                : "Failed to send the refresh request.";
            return sent;
        }
    }
}
