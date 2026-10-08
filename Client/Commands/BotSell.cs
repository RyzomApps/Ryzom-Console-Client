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
    /// Sell an item from the inventory to the current vendor.
    /// </summary>
    public class BotSell : CommandBase
    {
        public override string CmdName => "BotSell";

        public override string CmdUsage => "<bagIndex> [quantity] [price]";

        public override string CmdDesc => "Sell a bag item to the vendor in front of the player. Use 'BotTradeStart item' first, then this command. Slot index comes from 'Inventory'";

        public override bool Run(IClient handler, string command, out string responseMsg, Dictionary<string, object> localVars)
        {
            responseMsg = "";
            if (handler is not RyzomClient ryzomClient)
                throw new Exception("Command handler is not a Ryzom client.");

            var args = GetArgs(command);
            if (args.Length is not (1 or 2 or 3))
            {
                responseMsg = $"Usage: {CmdUsage}";
                return false;
            }

            var bagIndex = (ushort)Convert.ToInt32(args[0], 10);
            var quantity = args.Length >= 2 ? (ushort)Convert.ToInt32(args[1], 10) : (ushort)1;
            var price = args.Length == 3 ? (uint)Convert.ToInt32(args[2], 10) : (uint)1;

            var botChatManager = ryzomClient.GetBotChatManager();
            if (botChatManager.CurrentSessionId == 0)
            {
                responseMsg = "No trade session running. Use 'BotTradeStart item' first.";
                return false;
            }

            var entry = ryzomClient.GetInventoryManager().GetBagEntries().Find(e => e.Index == bagIndex);
            if (entry == null)
            {
                responseMsg = $"Bag slot {bagIndex} is empty. Use 'Inventory' to list the bag slots.";
                return false;
            }

            ryzomClient.GetStringManager().GetString(entry.Item.GetNameId(), out var name, ryzomClient.GetNetworkManager());
            if (string.IsNullOrEmpty(name))
                name = $"sheet {entry.Item.GetSheetId()}";

            if (quantity > entry.Item.GetQuantity())
            {
                responseMsg = $"Only {entry.Item.GetQuantity()} x {name} in the bag (asked: {quantity}).";
                return false;
            }

            var sent = botChatManager.SendSell(4, bagIndex, quantity, price);
            responseMsg = sent ? $"Sell requested: {quantity} x {name} (bag slot {bagIndex}, unit price {MoneyFormat.Format(price)})." : "Failed to send the sell request.";
            return sent;
        }
    }
}
