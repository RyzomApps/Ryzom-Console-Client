///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

using Client.Inventory;
using Client.Stream;
using static Client.Inventory.Inventories;

namespace Client.Network
{
    public class OneProp
    {
        public ItemPropId ItemPropId => (ItemPropId)ItemPropIdUint32;

        public uint ItemPropIdUint32;

        public int ItemPropValue = new int();

        public void Serial(BitMemoryStream bms)
        {
            bms.Serial(ref ItemPropIdUint32, (int)NbBitsForItemPropId);
            uint itemPropValue = (uint)ItemPropValue;
            bms.Serial(ref itemPropValue, (int)ItemSlot.DataBitSize[ItemPropIdUint32]);
            ItemPropValue = (int)itemPropValue;
        }
    }

    /// <summary>
    /// Slot in inventory (bag, room, etc.)
    /// </summary>
    public class ItemSlot
    {
        public static string[] ItemPropStr = new string[] { "SHEET", "QUALITY", "QUANTITY", "USER_COLOR", "CREATE_TIME", "SERIAL", "LOCKED", "WEIGHT", "NAMEID", "ENCHANT", "RM_CLASS_TYPE", "RM_FABER_STAT_TYPE", "PRICE", "RESALE_FLAG", "PREREQUISIT_VALID", "WORNED" };
        public static uint[] DataBitSize = new uint[] { 32, 10, 10, 3, 32, 32, 10, 16, 32, 10, 3, 5, 32, 2, 1, 1 };

        /// All item properties
        public int[] _ItemProp = new int[(int)ItemPropId.NbItemPropId];

        /// Only one prop (needs to be here because a CItemSlot can't be stored in an union because of the constructors)
        public OneProp _OneProp;

        /// Slot number
        private uint _SlotIndex;

        /// <summary>
        /// Return the version
        /// </summary>
        public static ushort GetVersion()
        {
            return 0;
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public ItemSlot() { }

        /// <summary>
        /// Constructor. Warning: does not reset the values!
        /// </summary>
        public ItemSlot(uint slotIndex)
        {
            _SlotIndex = slotIndex;
        }

        /// <summary>
        /// Change the slot index
        /// </summary>
        public void SetSlotIndex(uint slotIndex)
        {
            _SlotIndex = slotIndex;
        }

        /// <summary>
        /// Return the slot index
        /// </summary>
        public uint GetSlotIndex()
        {
            return _SlotIndex;
        }

        /// <summary>
        /// Set all properties to 0
        /// </summary>
        public void Reset()
        {
            for (uint i = 0; i != (int)ItemPropId.NbItemPropId; ++i)
            {
                _ItemProp[i] = 0;
            }
        }

        /// <summary>
        /// Set a property
        /// </summary>
        public void SetItemProp(ItemPropId id, int value)
        {
            _ItemProp[(int)id] = value;
        }

        /// <summary>
        /// Get a property
        /// </summary>
        public int GetItemProp(Inventories.ItemPropId id)
        {
            return _ItemProp[(int)id];
        }

        /// <summary>
        /// Serial from/to bit stream
        /// </summary>
        public void SerialAll(BitMemoryStream bms, InventoryCategoryTemplate template)
        {
            bms.Serial(ref _SlotIndex, (int)template.SlotBitSize);
            uint i;
            // SHEET, QUALITY, QUANTITY, USER_COLOR, CREATE_TIME and SERIAL never require compression
            for (i = 0; i < 6; ++i)
            {
                uint itemProp = (uint)_ItemProp[i];
                bms.Serial(ref itemProp, (int)DataBitSize[i]);
                _ItemProp[i] = (int)itemProp;
            }

            // For all other the compression is simple the first bit indicates if the value is zero
            if (bms.IsReading())
            {
                for (; i < (int)ItemPropId.NbItemPropId; ++i)
                {
                    bool b = new bool();
                    bms.Serial(ref b);

                    if (b)
                    {
                        _ItemProp[i] = 0;
                    }
                    else
                    {
                        uint itemProp = (uint)_ItemProp[i];
                        bms.Serial(ref itemProp, (int)DataBitSize[i]);
                        _ItemProp[i] = (int)itemProp;
                    }
                }
            }
            else
            {
                for (; i != (int)ItemPropId.NbItemPropId; ++i)
                {
                    bool b = (_ItemProp[i] == 0);

                    bms.Serial(ref b);
                    if (!b)
                    {
                        uint itemProp = (uint)_ItemProp[i];
                        bms.Serial(ref itemProp, (int)DataBitSize[i]);
                        _ItemProp[i] = (int)itemProp;
                    }
                }
            }
        }

        /// <summary>
        /// Serial from/to bit stream
        /// </summary>
        public void SerialOneProp(BitMemoryStream bms, InventoryCategoryTemplate template)
        {
            bms.Serial(ref _SlotIndex, (int)template.SlotBitSize);
            _OneProp = new OneProp();
            _OneProp.Serial(bms);
        }

        /// <summary>
        /// Set all properties from another object
        /// </summary>
        public void CopyFrom(ItemSlot src)
        {
            for (uint i = 0; i != (int)ItemPropId.NbItemPropId; ++i)
            {
                _ItemProp[i] = src._ItemProp[i];
            }
        }

        /// <summary>
        /// Accessors (for internal use only)
        /// </summary>
        public OneProp GetOneProp()
        {
            return _OneProp;
        }

    }
}