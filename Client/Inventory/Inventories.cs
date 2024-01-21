///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

namespace Client.Inventory
{
    public class Inventories
    {
        //const uint NbBitsForInventoryId = 3; // must include InvalidInvId
        /* ******
		WARNING!!!!! If you change those value, you'll have to:
		- change database.xml AND local_database.xml
		- change slotids server<->client sizes
		* *******/
        public const uint NbBagSlots = 500;
        public const uint NbPackerSlots = 500;
        public const uint NbRoomSlots = 1000;
        public const uint NbGuildSlots = 1000;
        public const uint NbTempInvSlots = 16;
        public const uint NbHotbarSlots = 5;

        public enum ItemPropId
        {
            Sheet,
            Quality,
            Quantity,
            UserColor,
            CreateTime,
            Serial,
            Locked,
            Weight,
            NameId,
            Enchant,
            ItemClass,
            ItemBestStat,
            Price,
            ResaleFlag,
            PrerequisitValid,
            Worned,
            NbItemPropId
        };

        public const uint NbBitsForItemPropId = 4; // TODO: replace this constant by an inline function using NbItemPropId

        public const uint LowNumberBound = 7;
        public const uint LowNumberBits = 3;
        public const uint InfoVersionBitSize = 8;

        public static string InfoVersionStr = "INFO_VERSION";

        public const uint MAX_PACK_ANIMAL = 3;
        public const uint MAX_MEKTOUB_MOUNT = 1;
        public const uint MAX_OTHER_PET = 3;
        public const uint MAX_INVENTORY_ANIMAL = (MAX_PACK_ANIMAL + MAX_MEKTOUB_MOUNT + MAX_OTHER_PET);

        public const uint MaxNbPackers = MAX_INVENTORY_ANIMAL;
    }
}
