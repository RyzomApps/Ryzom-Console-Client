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
using Client.Sheet;
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

        public override string CmdDesc => "List inventory items (bag by default, 16 per page). Inventories: bag, temp, pa0-pa6 (pack animals, mounts, pets), share, room, guild. Worn/held bag items are marked [EQUIPPED]. Slot indexes are used by 'BotSell'";

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
            var inventoryManager = ryzomClient.GetInventoryManager();
            var entries = inventoryManager.GetInventoryEntries(dbPath);

            // Bag slots referenced by the equipment/hand indirection arrays
            // (0-based bag child indices, -1 = empty) are worn or held.
            var equippedSlots = new HashSet<int>();
            if (inventoryName.Equals("bag", StringComparison.OrdinalIgnoreCase))
            {
                foreach (var slot in inventoryManager.ServerHands)
                    if (slot >= 0) equippedSlots.Add(slot);
                foreach (var slot in inventoryManager.ServerEquip)
                    if (slot >= 0) equippedSlots.Add(slot);
            }

            // Slots: used vs server maximum (inventories.h: NbBagSlots=500,
            // NbTempInvSlots=16, NbPackerSlots=500, NbRoomSlots=1000, NbGuildSlots=10000).
            var maxSlots = inventoryName.ToLowerInvariant() switch
            {
                "bag" => 500u,
                "temp" => 16u,
                "pa0" or "pa1" or "pa2" or "pa3" or "pa4" or "pa5" or "pa6" => 500u,
                "room" => 1000u,
                "guild" => 10000u,
                _ => 0u, // share: server-side slot count not mirrored
            };

            // Bulk: sum of sheet bulk * quantity over all slots, like the
            // official client (CInventoryManager::getBranchBulk sums
            // CItemSheet::Bulk * quantity). Max comes from the server DB
            // (inventory_manager.cpp: SERVER:STATIC_DATA:BAG_BULK_MAX,
            // SERVER:PACK_ANIMAL:BEASTn:BULK_MAX, SERVER:ROOM:INVENTORY:BULK_MAX).
            var usedBulk = 0.0;
            foreach (var entry in entries)
            {
                var sheet = ryzomClient.GetSheetManager().Get(ryzomClient.GetSheetIdFactory().SheetId(entry.Item.GetSheetId())) as ItemSheet;
                if (sheet != null)
                {
                    var qty = entry.Item.GetQuantity();
                    usedBulk += Math.Max(1, (int)qty) * sheet.Bulk; // client sums max(1, quantity) * CItemSheet::Bulk (getBranchBulk)
                }
            }

            var bulkMaxNodePath = inventoryName.ToLowerInvariant() switch
            {
                "bag" => "SERVER:STATIC_DATA:BAG_BULK_MAX",
                "pa0" => "SERVER:PACK_ANIMAL:BEAST0:BULK_MAX",
                "pa1" => "SERVER:PACK_ANIMAL:BEAST1:BULK_MAX",
                "pa2" => "SERVER:PACK_ANIMAL:BEAST2:BULK_MAX",
                "pa3" => "SERVER:PACK_ANIMAL:BEAST3:BULK_MAX",
                "pa4" => "SERVER:PACK_ANIMAL:BEAST4:BULK_MAX",
                "pa5" => "SERVER:PACK_ANIMAL:BEAST5:BULK_MAX",
                "pa6" => "SERVER:PACK_ANIMAL:BEAST6:BULK_MAX",
                "room" => "SERVER:ROOM:INVENTORY:BULK_MAX",
                _ => null,
            };

            var bulkMax = 0.0;
            if (bulkMaxNodePath != null)
            {
                var node = ryzomClient.GetDatabaseManager().GetServerNode(bulkMaxNodePath, false);
                if (node != null)
                    bulkMax = node.GetValue32(); // raw DB unit (server writes MaxPlayerBulk/1000; UI prints raw values too, see getBulkStrUserFct)
            }

            var slotsInfo = maxSlots > 0 ? $" {entries.Count}/{maxSlots}" : $" {entries.Count}";
            var bulkInfo = bulkMax > 0 ? $" {usedBulk:0.##}/{bulkMax:0.##}" : $" {usedBulk:0.##}";

            if (entries.Count == 0)
            {
                responseMsg = $"The {displayName} inventory is empty (or the data has not been received yet). Slots:{slotsInfo}, Bulk:{bulkInfo}";
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
                $"{displayName}: {entries.Count} items, Slots:{slotsInfo}, Bulk:{bulkInfo}, page {page}/{(entries.Count + (int)perPage - 1) / perPage} (use 'Inventory {inventoryName} <page>' for more)",
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
                if (item.GetWorned())
                    name += " [BROKEN]";
                if (equippedSlots.Contains((int)entry.Index))
                    name += " [EQUIPPED]";

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
