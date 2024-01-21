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
    public class InventoryCategoryForGuild : InventoryCategoryTemplate
    {
        //public new enum InventoryId
        //{
        //    GuildInvId,
        //    InvalidInvId,
        //    NbInventoryIds = InvalidInvId
        //}

        // Other values to change according to this InventoryNbSlots:
        // - game_share.h/inventories.h: CInventoryCategoryForGuild::SlotBitSize
        // - data_common/database.xml: GUILD:INVENTORY count
        // - data/gamedev/interfaces_v3/inventory.xml: inventory:content:guild param inv_branch_nb
        public string[] InventoryStr { get; } = new string[] { "GUILD" };
        public uint[] InventoryNbSlots { get; } = new uint[] { Inventories.NbGuildSlots };

        public uint SlotBitSize { get; } = 10;

        public uint NbInventoryIds => (uint)InventoryStr.Length;

        /// <summary>
        /// Return the inventory db root string
        /// </summary>
        public string GetDbStr(uint invId)
        {
            return ToString(invId) + ":INVENTORY";
        }

        public bool NeedPlainInfoVersionTransfer()
        {
            // incrementation is not sufficient because can incremented when player offline, and some values are skipped
            return true;
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

