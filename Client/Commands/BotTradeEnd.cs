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
    /// End the current trade session.
    /// </summary>
    public class BotTradeEnd : CommandBase
    {
        public override string CmdName => "BotTradeEnd";

        public override string CmdUsage => "";

        public override string CmdDesc => "End the current trade session (BOTCHAT:END); a new session must be started afterwards";

        public override bool Run(IClient handler, string command, out string responseMsg, Dictionary<string, object> localVars)
        {
            responseMsg = "";
            if (handler is not RyzomClient ryzomClient)
                throw new Exception("Command handler is not a Ryzom client.");

            var botChatManager = ryzomClient.GetBotChatManager();
            if (botChatManager.CurrentSessionId == 0)
            {
                responseMsg = "No trade session running.";
                return false;
            }

            var sent = botChatManager.SendEndTrade();
            responseMsg = sent ? "Trade session ended." : "Failed to send the end request.";
            return sent;
        }
    }
}
