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
    public class InventoryCategoryForCharacter : InventoryCategoryTemplate
    {
        //public new enum InventoryId
        //{
        //    Bag,
        //    Packers,
        //    Room = Packers + (int)Inventories.MaxNbPackers,
        //    InvalidInvId,
        //    NbInventoryIds = InvalidInvId
        //}

        // Other values to change according to these InventoryNbSlots:
        // - game_share.h/inventories.h: CInventoryCategoryForCharacter::SlotBitSize
        // - data_common/database.xml: INVENTORY:BAG count
        // - data/gamedev/interfaces_v3/inventory.xml: inventory:content:bag param inv_branch_nb
        public string[] InventoryStr { get; } = new string[] { "BAG", "PACK_ANIMAL0", "PACK_ANIMAL1", "PACK_ANIMAL2", "PACK_ANIMAL3", "PACK_ANIMAL4", "PACK_ANIMAL5", "PACK_ANIMAL6", "ROOM" };
        public uint[] InventoryNbSlots { get; } = new uint[] { Inventories.NbBagSlots, Inventories.NbPackerSlots, Inventories.NbPackerSlots, Inventories.NbPackerSlots, Inventories.NbPackerSlots, Inventories.NbPackerSlots, Inventories.NbPackerSlots, Inventories.NbPackerSlots, Inventories.NbRoomSlots };

        public uint SlotBitSize { get; } = 10;

        public uint NbInventoryIds => (uint)InventoryStr.Length;

        /// <summary>
        /// Return the inventory db root string
        /// </summary>
        public string GetDbStr(uint invId)
        {
            return $"INVENTORY:{ToString(invId)}";
        }

        public bool NeedPlainInfoVersionTransfer()
        {
            // incrementation is sufficient
            return false;
        }

        /// <summary>
        /// alternate version for TInventoryId
        /// </summary>
        private string ToString(uint invId)
        {
            return InventoryStr[(int)invId];
        }
    }
}

