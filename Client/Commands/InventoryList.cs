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
using API.Helper;
using API.Inventory;
using System;
using System.Collections.Generic;

namespace Client.Commands
{
    /// <summary>
    /// List the items of the bag, page by page.
    /// </summary>
    public class InventoryList : CommandBase
    {

        public override CommandCategory CmdCategory => CommandCategory.Inventory;
        private const uint DefaultPerPage = 16;

        public override string CmdName => "Inventory";

        public override string CmdUsage => "[page] [perPage]";

        public override string CmdDesc => "List the bag items (16 per page by default). Slot indexes are used by 'BotSell'";

        public override bool Run(IClient handler, string command, out string responseMsg, Dictionary<string, object> localVars)
        {
            responseMsg = "";
            if (handler is not RyzomClient ryzomClient)
                throw new Exception("Command handler is not a Ryzom client.");

            var args = GetArgs(command);
            var page = args.Length >= 1 ? Convert.ToInt32(args[0], 10) : 1;
            var perPage = args.Length == 2 ? (uint)Convert.ToInt32(args[1], 10) : DefaultPerPage;

            if (page < 1 || perPage < 1)
            {
                responseMsg = $"Usage: {CmdUsage}";
                return false;
            }

            var entries = ryzomClient.GetInventoryManager().GetBagEntries();

            if (entries.Count == 0)
            {
                responseMsg = "The bag is empty (or the bag data has not been received yet).";
                return true;
            }

            var first = (page - 1) * (int)perPage;
            if (first >= entries.Count)
            {
                responseMsg = $"Page {page} is out of range ({entries.Count} items, {perPage} per page).";
                return false;
            }

            var lines = new List<string>
            {
                $"Bag: {entries.Count} items, page {page}/{(entries.Count + (int)perPage - 1) / perPage} (use 'Inventory <page>' for more)",
                $"{"Slot",-5} {"Qty",-5} {"Q",-3} {"Weight",-8} Name"
            };

            // Pre-request display strings for all visible items; the server
            // answer lands in the string manager cache and is available on the
            // next run (see comment on sheet_id.bin fallback).
            ItemNameResolver.RequestNames(handler, EnumerateNameIds(entries, first, (int)perPage));

            for (var i = first; i < first + perPage && i < entries.Count; i++)
            {
                var entry = entries[i];
                var item = entry.Item;

                var name = ItemNameResolver.Resolve(handler, item.GetNameId(), item.GetSheetId());

                lines.Add($"{entry.Index,-5} {item.GetQuantity(),-5} {item.GetQuality(),-3} {item.GetWeight(),-8} {name}");
            }

            responseMsg = string.Join("\n", lines);
            return true;
        }

        private static IEnumerable<uint> EnumerateNameIds(IList<IBagEntry> entries, int first, int perPage)
        {
            for (var i = first; i < first + perPage && i < entries.Count; i++)
                yield return entries[i].Item.GetNameId();
        }
    }
}
