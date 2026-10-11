///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////
using System;

namespace Client.Inventory
{
    /// <summary>
    /// Maps between the wire slot id used by ITEM_INFO:* impulses and the
    /// inventory index + slot pair, and between the inventory index and its
    /// server database branch.
    ///
    /// The wire id is (inventoryIndex &lt;&lt; 11) | slot (item_infos.h:64,
    /// ITEM_ORG macro: 11 bits for the slot so the trading list fits in
    /// INVENTORIES::NB_ITEM_PER_INV entries).
    /// </summary>
    public static class ItemSlotId
    {
        /// <summary>11 bits hold the slot index (item_infos.h ITEM_ORG).</summary>
        public const uint SlotBits = 11;

        // game_share INVENTORIES::TInventory indexes (inventories.h)
        public const uint InventoryHandling = 0;
        public const uint InventoryTemporary = 1;
        public const uint InventoryEquipment = 2;
        public const uint InventoryHotbar = 3;
        public const uint InventoryBag = 4;
        public const uint InventoryPackAnimal = 5; // 5..11 => pack animal 0..6
        public const uint InventoryMaxPackAnimal = 11;
        public const uint InventoryExchange = 13;
        public const uint InventoryExchangeProposition = 14;
        public const uint InventoryTrading = 15;
        public const uint InventoryRewardSharing = 16;
        public const uint InventoryGuild = 17;
        public const uint InventoryPlayerRoom = 18;

        /// <summary>Build the wire slot id from inventory index and slot.</summary>
        public static uint Encode(uint inventoryIndex, uint slot)
        {
            return (inventoryIndex << (int)SlotBits) | (slot & ((1u << (int)SlotBits) - 1));
        }

        /// <summary>Split a wire slot id into inventory index and slot.</summary>
        public static void Decode(uint slotId, out uint inventoryIndex, out uint slot)
        {
            inventoryIndex = slotId >> (int)SlotBits;
            slot = slotId & ((1u << (int)SlotBits) - 1);
        }

        /// <summary>
        /// Server database branch of an inventory (paths as in
        /// inventory_manager.cpp InventoryDBs, without the bank prefix —
        /// GetServerBranch adds it).
        /// </summary>
        public static string DbPath(uint inventoryIndex)
        {
            switch (inventoryIndex)
            {
                case InventoryTemporary: return "INVENTORY:TEMP";
                case InventoryBag: return "INVENTORY:BAG";
                case InventoryPackAnimal: return "INVENTORY:PACK_ANIMAL";
                default:
                    if (inventoryIndex >= InventoryPackAnimal && inventoryIndex <= InventoryMaxPackAnimal)
                        return $"INVENTORY:PACK_ANIMAL{inventoryIndex - InventoryPackAnimal}";
                    return null;
                case InventoryExchange: return "EXCHANGE:GIVE";
                case InventoryExchangeProposition: return "EXCHANGE:RECEIVE";
                case InventoryTrading: return "TRADING";
                case InventoryRewardSharing: return "INVENTORY:SHARE";
                case InventoryGuild: return "GUILD:INVENTORY";
                case InventoryPlayerRoom: return "INVENTORY:ROOM";
            }
        }

        /// <summary>
        /// Resolve an inventory argument to its TInventory index. Accepts the
        /// common names (bag, temp, pet0..pet6, give, receive, trading, share,
        /// guild, room) or the raw numeric index 0..18.
        /// </summary>
        public static bool TryResolveInventory(string arg, out uint inventoryIndex)
        {
            switch ((arg ?? "").ToLowerInvariant())
            {
                case "bag": inventoryIndex = InventoryBag; return true;
                case "temp": inventoryIndex = InventoryTemporary; return true;
                case "give": inventoryIndex = InventoryExchange; return true;
                case "receive": inventoryIndex = InventoryExchangeProposition; return true;
                case "trading": inventoryIndex = InventoryTrading; return true;
                case "share": inventoryIndex = InventoryRewardSharing; return true;
                case "guild": inventoryIndex = InventoryGuild; return true;
                case "room": inventoryIndex = InventoryPlayerRoom; return true;
                default:
                    if (arg != null && arg.StartsWith("pet", StringComparison.OrdinalIgnoreCase) &&
                        int.TryParse(arg.Substring(3), out var pet) && pet >= 0 && pet <= (int)(InventoryMaxPackAnimal - InventoryPackAnimal))
                    {
                        inventoryIndex = InventoryPackAnimal + (uint)pet;
                        return true;
                    }
                    if (uint.TryParse(arg, out var index) && index <= InventoryPlayerRoom)
                    {
                        inventoryIndex = index;
                        return true;
                    }
                    inventoryIndex = 0;
                    return false;
            }
        }
    }
}
