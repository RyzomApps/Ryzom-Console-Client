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

        public override string CmdUsage => "[inventory] [page] [perPage]";

        public override string CmdDesc => "List inventory items (bag by default, 16 per page). Inventories: bag, temp, pa0-pa6 (pack animals, mounts, pets), share, room, guild. Slot indexes are used by 'BotSell'";

        /// <summary>
        /// Named inventory -> server database branch. Bag is the default.
        /// </summary>
        private static readonly Dictionary<string, string> InventoryPaths = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "bag",   "INVENTORY:BAG" },
            { "temp",  "INVENTORY:TEMP" },
            { "pa0",   "INVENTORY:PACK_ANIMAL0" },
            { "pa1",   "INVENTORY:PACK_ANIMAL1" },
            { "pa2",   "INVENTORY:PACK_ANIMAL2" },
            { "pa3",   "INVENTORY:PACK_ANIMAL3" },
            { "pa4",   "INVENTORY:PACK_ANIMAL4" },
            { "pa5",   "INVENTORY:PACK_ANIMAL5" },
            { "pa6",   "INVENTORY:PACK_ANIMAL6" },
            { "share", "INVENTORY:SHARE" },
            { "room",  "INVENTORY:ROOM" },
            { "guild", "INVENTORY:GUILD" },
        };

        public override bool Run(IClient handler, string command, out string responseMsg, Dictionary<string, object> localVars)
        {
            responseMsg = "";
            if (handler is not RyzomClient ryzomClient)
                throw new Exception("Command handler is not a Ryzom client.");

            var args = GetArgs(command);

            // Optional first arg: inventory name (default: bag). Numeric args
            // are page/perPage as before.
            var inventoryName = "bag";
            var argIndex = 0;
            if (args.Length > 0 && !int.TryParse(args[0], out _))
            {
                inventoryName = args[0];
                argIndex = 1;
            }

            if (!InventoryPaths.TryGetValue(inventoryName, out var dbPath))
            {
                responseMsg = $"Unknown inventory '{inventoryName}'. Available: {string.Join(", ", InventoryPaths.Keys)}";
                return false;
            }

            var page = args.Length >= argIndex + 1 ? Convert.ToInt32(args[argIndex], 10) : 1;
            var perPage = args.Length == argIndex + 2 ? (uint)Convert.ToInt32(args[argIndex + 1], 10) : DefaultPerPage;

            if (page < 1 || perPage < 1)
            {
                responseMsg = $"Usage: {CmdUsage}";
                return false;
            }

            var displayName = inventoryName == "bag" ? "Bag" : inventoryName.ToUpperInvariant();
            var entries = ryzomClient.GetInventoryManager().GetInventoryEntries(dbPath);

            if (entries.Count == 0)
            {
                responseMsg = $"The {displayName} inventory is empty (or the data has not been received yet).";
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
                $"{displayName}: {entries.Count} items, page {page}/{(entries.Count + (int)perPage - 1) / perPage} (use 'Inventory {inventoryName} <page>' for more)",
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
