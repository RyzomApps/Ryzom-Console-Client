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
    /// Buy an item of the trade list.
    /// </summary>
    public class BotTradeBuy : CommandBase
    {
        public override string CmdName => "BotTradeBuy";

        public override string CmdUsage => "<index> [quantity]";

        public override string CmdDesc => "Buy an entry of the current trade list page by its slot index (0..7, default quantity 1)";

        public override bool Run(IClient handler, string command, out string responseMsg, Dictionary<string, object> localVars)
        {
            responseMsg = "";
            if (handler is not RyzomClient ryzomClient)
                throw new Exception("Command handler is not a Ryzom client.");

            var args = GetArgs(command);
            if (args.Length is not (1 or 2))
            {
                responseMsg = $"Usage: {CmdUsage}";
                return false;
            }

            var botChatManager = ryzomClient.GetBotChatManager();
            if (botChatManager.CurrentSessionId == 0)
            {
                responseMsg = "No trade session running. Use 'BotTradeStart <type>' first.";
                return false;
            }

            var index = (byte)Convert.ToInt32(args[0], 10);
            var quantity = args.Length == 2 ? (ushort)Convert.ToInt32(args[1], 10) : (ushort)1;

            var list = botChatManager.GetTradeList();
            TradeEntry? entry = null;
            foreach (var tradeEntry in list)
            {
                if (tradeEntry.Index == index)
                    entry = tradeEntry;
            }

            if (entry == null)
            {
                responseMsg = $"No entry on slot {index} of the current page. Use 'BotTradeList' to see the slots.";
                return false;
            }

            ryzomClient.GetStringManager().GetString(entry.Value.NameId, out var name, ryzomClient.GetNetworkManager());
            var sent = botChatManager.SendBuyItem(index, quantity);
            responseMsg = sent ? $"Buy requested: {quantity} x {name} (slot {index})." : "Failed to send the buy request.";
            return sent;
        }
    }
}
