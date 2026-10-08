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
    /// Start a bot chat trade session (item, faction, teleport, skill, pact or action trade).
    /// </summary>
    public class BotTradeStart : CommandBase
    {
        public override string CmdName => "BotTradeStart";

        public override string CmdUsage => "<item|faction|teleport|skill|pact|action>";

        public override string CmdDesc => "Start a trade session with the bot currently in front of the player";

        public override bool Run(IClient handler, string command, out string responseMsg, Dictionary<string, object> localVars)
        {
            responseMsg = "";
            if (handler is not RyzomClient ryzomClient)
                throw new Exception("Command handler is not a Ryzom client.");

            var args = GetArgs(command);
            if (args.Length != 1)
            {
                responseMsg = $"Usage: {CmdUsage}";
                return false;
            }

            var botChatManager = ryzomClient.GetBotChatManager();
            var sent = args[0].ToLowerInvariant() switch
            {
                "item" => botChatManager.SendStartTradeItem(),
                "faction" => botChatManager.SendStartTradeFaction(),
                "teleport" => botChatManager.SendStartTradeTeleport(),
                "skill" => botChatManager.SendStartTradeSkill(),
                "pact" => botChatManager.SendStartTradePact(),
                "action" => botChatManager.SendStartTradeAction(),
                _ => false
            };

            if (!sent)
            {
                responseMsg = $"Unknown trade type '{args[0]}'. Usage: {CmdUsage}";
                return false;
            }

            // The server answers with the trade list in SERVER:TRADING; give
            // the player the command to display it once it has arrived.
            responseMsg = $"Trade session started (type: {args[0]}, session: {botChatManager.CurrentSessionId}). " +
                          "Use 'BotTradeList' to display the current page.";
            return true;
        }
    }
}
