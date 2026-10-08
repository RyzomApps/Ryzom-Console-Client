///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
using API;
using API.Commands;
using Client.Stream;
using System;
using System.Collections.Generic;
using System.Text;

namespace Client.Commands
{
    /// <summary>
    /// Put an item from the bag into the player trade window.
    /// Sends EXCHANGE:ADD (inventory_src, index_in_bag, index_in_exchange, quantity).
    /// </summary>
    public class ExchangePutItem : CommandBase
    {
        public override string CmdName => "ExchangePutItem";

        public override string CmdUsage => "<bagIndex> [quantity] [exchangeSlot]";

        public override string CmdDesc => "Put an item from the bag into the player trade. Use 'ExchangeProposal' first; slot indices come from 'Inventory'";

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
            var exchangeSlot = args.Length == 3 ? (ushort)Convert.ToInt32(args[2], 10) : byte.MaxValue;

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

            // Find the first free exchange slot if none was given
            if (exchangeSlot == byte.MaxValue)
            {
                exchangeSlot = byte.MaxValue;
                for (var i = 0; i < 10; i++)
                {
                    var sheet = ExchangeDb.SafeProp(ryzomClient, $"SERVER:EXCHANGE:GIVE:{i}:SHEET", -1);
                    if (sheet == 0)
                    {
                        exchangeSlot = (byte)i;
                        break;
                    }
                }

                if (exchangeSlot == byte.MaxValue)
                {
                    responseMsg = "No free exchange slot. Use 'ExchangeRemoveItem' or 'ExchangeInvalidate' first.";
                    return false;
                }
            }

            const string msgName = "EXCHANGE:ADD";
            var out2 = new BitMemoryStream();
            if (!ryzomClient.GetNetworkManager().GetMessageHeaderManager().PushNameToStream(msgName, out2))
            {
                responseMsg = $"Unknown message name '{msgName}'.";
                return false;
            }

            ushort bagInvId = 4; // CInventoryCategory::bag
            var exchangeSlotByte = (byte)exchangeSlot;
            out2.Serial(ref bagInvId);
            out2.Serial(ref bagIndex);
            out2.Serial(ref exchangeSlotByte); // dest slot is u8 in the server message
            out2.Serial(ref quantity);

            ryzomClient.GetNetworkManager().Push(out2);

            responseMsg = $"Put requested: {quantity} x {name} (bag slot {bagIndex}) into exchange slot {exchangeSlot}. Use 'ExchangeShow' to check.";
            return true;
        }
    }
}
