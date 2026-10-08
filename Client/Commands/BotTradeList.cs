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
    /// Display the trade list of the current trade session, page by page.
    /// </summary>
    public class BotTradeList : CommandBase
    {
        private static readonly string[] CurrencyNames = { "dappers", "faction", "tp", "skill", "guild", "cosmetic", "unknown" };

        public override string CmdName => "BotTradeList";

        public override string CmdUsage => "[page]";

        public override string CmdDesc => "Display the current trade list page (8 slots per page; the server fills one page at a time)";

        public override bool Run(IClient handler, string command, out string responseMsg, Dictionary<string, object> localVars)
        {
            responseMsg = "";
            if (handler is not RyzomClient ryzomClient)
                throw new Exception("Command handler is not a Ryzom client.");

            GetArgs(command); // the optional argument is ignored: the server fills the DB page by page

            var botChatManager = ryzomClient.GetBotChatManager();
            var list = botChatManager.GetTradeList();

            if (botChatManager.CurrentSessionId == 0)
            {
                responseMsg = "No trade session running. Use 'BotTradeStart <type>' first.";
                return false;
            }

            if (list.Count == 0)
            {
                responseMsg = "The trade list is empty. Wait for the server answer and run 'BotTradeList' again.";
                return true;
            }

            var lines = new List<string>
            {
                $"Trade page {botChatManager.PageId}" +
                (botChatManager.HasNextPage ? " (more pages: use 'BotTradeNext')" : " (last page)"),
                $"{"Slot",-5} {"Sheet",-10} {"Q",-3} {"Qty",-6} {"Price",-12} {"Cur",-8} {"Type",-4} Name"
            };

            foreach (var entry in list)
            {
                ryzomClient.GetStringManager().GetString(entry.NameId, out var name, ryzomClient.GetNetworkManager());
                if (string.IsNullOrEmpty(name))
                    name = $"sheet {entry.SheetId}";
                if (!entry.PrerequisitValid)
                    name += " [locked]";

                var currency = entry.Currency < CurrencyNames.Length ? CurrencyNames[entry.Currency] : entry.Currency.ToString();

                lines.Add($"{entry.Index,-5} {entry.SheetId,-10} {entry.Quality,-3} {entry.Quantity,-6} {MoneyFormat.Format(entry.Price),-12} {currency,-8} {entry.SlotType,-4} {name}");
            }

            responseMsg = string.Join("\n", lines);
            return true;
        }
    }
}
