///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

using System;
using System.Diagnostics;
using System.IO;
using API.Sheet;
using Client.Database;
using Client.Sheet;
using Client.Stream;

namespace Client.Inventory
{
    /// <summary>
    /// This manager gives direct access to inventory slots (bag, temporary inventory, hands, and equip inventory)
    /// This also give access to player money
    /// </summary>
    /// <author>Nicolas Vizerie</author>
    /// <author>Nevrax France</author>
    /// <date>September 2003</date>
    public class InventoryManager
    {
        private readonly RyzomClient _client;

        const uint MAX_TEMPINV_ENTRIES = 16;
        const uint MAX_BAGINV_ENTRIES = 500;
        const uint MAX_HANDINV_ENTRIES = 2;
        const uint MAX_EQUIPINV_ENTRIES = 19;
        const uint MAX_ANIMALINV_ENTRIES = 500;
        //const uint MAX_GUILDINV_ENTRIES = 1000;
        //const uint MAX_ROOMINV_ENTRIES = 1000;

        const int MAX_PACK_ANIMAL = 3;
        const int MAX_MEKTOUB_MOUNT = 1;
        const int MAX_OTHER_PET = 3;

        const int MAX_INVENTORY_ANIMAL = (MAX_PACK_ANIMAL + MAX_MEKTOUB_MOUNT + MAX_OTHER_PET);

        // This is the personal player inventory max (bag and animal)
        //const uint MAX_PLAYER_INV_ENTRIES = 500;

        // db path for the local inventory
        //const string LOCAL_INVENTORY = "LOCAL:INVENTORY";
        const string INVENTORY = "INVENTORY";

        // db path for all the inventories (without the SERVER: prefix)
        private readonly string[] InventoryDBs = {
            "INVENTORY:BAG",
            "INVENTORY:PACK_ANIMAL0",
            "INVENTORY:PACK_ANIMAL1",
            "INVENTORY:PACK_ANIMAL2",
            "INVENTORY:PACK_ANIMAL3",
            "INVENTORY:TEMP",
            "EXCHANGE:GIVE",
            "EXCHANGE:RECEIVE",
            "TRADING",
            "INVENTORY:SHARE",
            "GUILD:INVENTORY",
            "INVENTORY:ROOM"
        };

        public enum INVENTORIES : ushort
        {
            // TODO : remove handling, merge it with equipement
            handling = 0,
            temporary, // 1
            equipment, // 2
            bag, // 3
            pet_animal, // 4 Character can have 5 pack animal
            pet_animal1 = pet_animal, // for toString => TInventory convertion
            pet_animal2,
            pet_animal3,
            pet_animal4,
            max_pet_animal, // 8
            NUM_INVENTORY = max_pet_animal, // 8
            UNDEFINED = NUM_INVENTORY, // 8

            exchange, // 9  This is not a bug : exchange is a fake inventory
            exchange_proposition, // 10  and should not count in the number of inventory
            // same for botChat trading.
            trading, // 11
            reward_sharing, // 12 fake inventory, not in database.xml. Used by the item info protocol only
            guild, // 13 (warning: number stored in guild saved file)
            player_room, // 14
            NUM_ALL_INVENTORY // warning: distinct from NUM_INVENTORY
        }

        readonly uint NumInventories = new uint();

        // SERVER INVENTORY
        private ItemImage[] ServerBag = new ItemImage[MAX_BAGINV_ENTRIES];
        private ItemImage[] ServerTempInv = new ItemImage[MAX_TEMPINV_ENTRIES];
        public int[] ServerHands = new int[MAX_HANDINV_ENTRIES];
        public int[] ServerEquip = new int[MAX_EQUIPINV_ENTRIES];
        public int[] ServerHotbar = new int[MAX_EQUIPINV_ENTRIES];
        private DatabaseNodeLeaf ServerMoney;
        private ItemImage[][] ServerPAInv = new ItemImage[MAX_INVENTORY_ANIMAL][];

        // Cache to know if bag is locked or not, because of item worn
        private bool[] BagItemEquipped = new bool[MAX_BAGINV_ENTRIES];

        // Equipment observer
        DatabaseEquipObs _DBEquipObs;

        // ***************************************************************************
        // db path for all the inventories (without the SERVER: prefix)

        public InventoryManager(RyzomClient client)
        {
            _client = client;
            _DBEquipObs = new DatabaseEquipObs(client);

            ServerMoney = null;

            uint i;

            for (i = 0; i < MAX_HANDINV_ENTRIES; ++i)
            {
                ServerHands[i] = 0;
            }

            for (i = 0; i < MAX_EQUIPINV_ENTRIES; ++i)
            {
                ServerEquip[i] = 0;
            }

            for (i = 0; i < MAX_BAGINV_ENTRIES; i++)
            {
                BagItemEquipped[i] = false;
            }

            //Debug.Assert(NumInventories == InventoryIndexes.Length);
        }

        public void Init()
        {
            // LOCAL DB is not implemented

            // SERVER DB
            InitItemArray($"{INVENTORY}:BAG", ServerBag, MAX_BAGINV_ENTRIES);
            InitItemArray($"{INVENTORY}:TEMP", ServerTempInv, MAX_TEMPINV_ENTRIES);
            ServerMoney = _client.GetDatabaseManager().GetServerNode($"{INVENTORY}:MONEY");

            // Init Animals
            for (uint i = 0; i < MAX_INVENTORY_ANIMAL; i++)
            {
                ServerPAInv[i] = new ItemImage[MAX_ANIMALINV_ENTRIES];
                InitItemArray($"{INVENTORY}:PACK_ANIMAL{i}", ServerPAInv[i], MAX_ANIMALINV_ENTRIES);
            }

            // Drag'n'Drop is not implemented

            // ItemInfoObservers are not implemented
        }

        /// <summary>
        /// Init an array of items from a db branch
        /// </summary>
        private void InitItemArray(in string dbBranchName, ItemImage[] dest, uint numItems)
        {
            Debug.Assert(dest != null);

            var branch = _client.GetDatabaseManager().GetServerBranch(dbBranchName);

            if (branch == null)
            {
                _client.Log.Warn($"Can't init inventory image from branch {dbBranchName}.");
                return;
            }

            for (uint k = 0; k < numItems; ++k)
            {
                if (!(branch.GetNode((ushort)k) is DatabaseNodeBranch itemBranch))
                {
                    _client.Log.Warn($"Can't retrieve item {k} of branch {dbBranchName}");
                }
                else
                {
                    dest[k] = new ItemImage();
                    dest[k].Build(itemBranch);
                }
            }
        }

        /// <summary>
        /// Called on impulse
        /// </summary>
        internal void OnUpdateEquipHands()
        {
            // update hands slots after initial BAG inventory has received
            var pNl = _client.GetDatabaseManager().GetServerNode($"{INVENTORY}:HAND:0:INDEX_IN_BAG", false);
            if (pNl != null && pNl.GetValue32() != 0)
            {
                _DBEquipObs.Update(pNl);
            }

            pNl = _client.GetDatabaseManager().GetServerNode($"{INVENTORY}:HAND:1:INDEX_IN_BAG", false);
            if (pNl != null && pNl.GetValue32() != 0)
            {
                _DBEquipObs.Update(pNl);
            }
        }

        /// <summary>
        /// Dump all inventory to a file.
        /// </summary>
        public void Write(string fileName)
        {
            var f = new StreamWriter(fileName, false);

            for (uint i = 0; i < MAX_BAGINV_ENTRIES; i++)
            {
                var sbi = GetServerBagItem(i);

                if (sbi == null || sbi.GetSheetId() == 0)
                    continue;

                var sheet = _client.GetApiSheetIdFactory().SheetId(sbi.GetSheetId());
                var name = "";
                var family = "";

                if (sbi.GetNameId() != 0)
                    _client.GetStringManager().GetDynString(sbi.GetNameId(), out name, _client.GetNetworkManager());

                if (sheet.GetSheetType() == (uint)SheetType.ITEM)
                    family = ((EntitySheet)sheet).Type.ToString();

                f.WriteLine($"{i}\t{sheet.Name}\t{family}\t{name}\t{sbi.GetQuality()}\t{sbi.GetQuantity()}");
            }

            f.Close();
        }

        internal void SortBag()
        {
            // Ignored, since there is no interface
        }

        public ItemImage GetServerHandItem(uint index)
        {
            Debug.Assert(index < MAX_HANDINV_ENTRIES);
            return ServerHands[index] != 0 ? ServerBag[ServerHands[index]] : null;
        }
        public ItemImage GetServerEquipItem(uint index)
        {
            Debug.Assert(index < MAX_EQUIPINV_ENTRIES);
            return ServerEquip[index] != 0 ? ServerBag[ServerEquip[index]] : null;
        }

        public ItemImage GetServerPAItem(uint beastIndex, uint index)
        {
            Debug.Assert(beastIndex < MAX_INVENTORY_ANIMAL);
            Debug.Assert(index < MAX_ANIMALINV_ENTRIES);
            return ServerPAInv[beastIndex][index];
        }

        public ItemImage GetServerBagItem(uint index)
        {
            Debug.Assert(index < MAX_BAGINV_ENTRIES);
            return ServerBag[index];
        }

        public ItemImage GetServerItem(uint inv, uint index)
        {
            if (inv == (uint)INVENTORIES.bag)
            {
                return GetServerBagItem(index);
            }
            if (inv >= (uint)INVENTORIES.pet_animal && inv < (uint)INVENTORIES.pet_animal + MAX_INVENTORY_ANIMAL)
            {
                return GetServerPAItem(inv - (uint)INVENTORIES.pet_animal, index);
            }

            _client.Log.Error("Not a bag or pet inventory.");

            return new ItemImage();
        }

        public ulong GetServerMoney()
        {
            return (ulong)(ServerMoney?.GetValue64() ?? 0);
        }

        public void WearBagItem(int bagEntryIndex)
        {
            if (bagEntryIndex < 0 || bagEntryIndex >= (int)MAX_BAGINV_ENTRIES)
                return;

            BagItemEquipped[bagEntryIndex] = true;
            SortBag();
        }

        public void UnwearBagItem(int bagEntryIndex)
        {
            if (bagEntryIndex < 0 || bagEntryIndex >= (int)MAX_BAGINV_ENTRIES)
                return;

            BagItemEquipped[bagEntryIndex] = false;
            SortBag();
        }


        public void Equip(in string bagPath, in string invPath)
        {
            if (bagPath.Length == 0 || invPath.Length == 0)
            {
                return;
            }

            // Get inventory and slot
            var sIndexInBag = bagPath.Substring(bagPath.LastIndexOf(':') + 1, bagPath.Length);
            ushort.TryParse(sIndexInBag, out var indexInBag);

            var inventory = (ushort)INVENTORIES.UNDEFINED;
            ushort invSlot = 0xffff;

            if (invPath.StartsWith("SERVER:INVENTORY:HAND", StringComparison.InvariantCultureIgnoreCase))
            {
                inventory = (ushort)INVENTORIES.handling;
                ushort.TryParse(invPath.Substring(21, invPath.Length), out invSlot);
            }
            else if (invPath.StartsWith("SERVER:INVENTORY:EQUIP", StringComparison.InvariantCultureIgnoreCase))
            {
                inventory = (ushort)INVENTORIES.equipment;
                ushort.TryParse(invPath.Substring(22, invPath.Length), out invSlot);
            }

            // Hands management: check if we have to unequip left hand because of incompatibility with right hand item
            var oldRightIndexInBag = _client.GetDatabaseManager().GetServerNode(invPath + ":INDEX_IN_BAG").GetValue16();

            // Local inventory handling not implemented

            // update the equip DB pointer
            _client.GetDatabaseManager().GetServerNode(invPath + ":INDEX_IN_BAG").SetValue16((short)(indexInBag + 1));

            // Phrase invalidation is not implemented

            // Bot trade is not implemented

            // Send message to the server
            if (inventory != (short)INVENTORIES.UNDEFINED)
            {
                var @out = new BitMemoryStream();
                const string sMsg = "ITEM:EQUIP";

                if (_client.GetNetworkManager().GetMessageHeaderManager().PushNameToStream(sMsg, @out))
                {
                    // Fill the message (equipped inventory, equipped inventory slot, bag slot)
                    @out.Serial(ref inventory);
                    @out.Serial(ref invSlot);
                    @out.Serial(ref indexInBag);
                    _client.GetNetworkManager().Push(@out);

                    // Local synch counter is not implemented

                    //nlinfo("impulseCallBack : %s %d %d %d sent", sMsg.c_str(), inventory, invSlot, indexInBag);
                }
                else
                {
                    _client.Log.Warn($"Don't know message name {sMsg}");
                }
            }
        }

        /// <summary>
        /// Auto equip an item (given by index) from the bag (return true if equipped)
        /// </summary>
        internal bool AutoEquip(int bagEntryIndex, bool allowReplace)
        {
            /*
            uint i;

            InterfaceManager pIM = InterfaceManager.getInstance();
            ListSheetBase pList = WidgetManager.getInstance().getElementFromId(LIST_BAG_TEXT) as IListSheetBase;
            DatabaseCtrlSheet pCSSrc = null;

            if (pList == null)
                return false;


            for (i = 0; i < MAX_BAGINV_ENTRIES; ++i)
            {
                pCSSrc = pList.getSheet(i);
                string sTmp = pCSSrc.getSheet();
                sTmp = sTmp.Substring(sTmp.LastIndexOf(':') + 1, sTmp.Length);
                sint nTmp = new sint();
                fromString(sTmp, nTmp);
                if (nTmp == bagEntryIndex)
                    break;

            }

            if (i == MAX_BAGINV_ENTRIES)
                return false;

            if (pCSSrc == null)
                return false;


            for (i = 0; i < MAX_HANDINV_ENTRIES; ++i)
            {
                DatabaseCtrlSheet pCSDst = getHandSheet(i);
                if (pCSDst == null)
                {
                    continue;
                }
                string dstPath = getDBIndexPath(pCSDst);

                int indexDstPath = NLGUI.CDBManager.getInstance().getDbProp(dstPath + ":INDEX_IN_BAG").getValue16();

                // Already something in that slot?
                if (!allowReplace && indexDstPath > 0)
                {
                    continue;
                }

                // Does the source and destination are items ?
                if (pCSSrc.getType() == CtrlSheetInfo.SheetType_Item)
                {
                    if (pCSDst.getType() == CtrlSheetInfo.SheetType_Item)
                    {
                        // Right Slot ?
                        if (pCSDst.canDropItem(pCSSrc))
                        {
                            // Ok let us equip with this item
                            string srcPath = pCSSrc.getSheet();
                            equip(srcPath, dstPath);
                            return true;
                        }
                    }
                }
            }

            for (i = 0; i < MAX_EQUIPINV_ENTRIES; ++i)
            {
                DatabaseCtrlSheet pCSDst = getEquipSheet(i);
                if (pCSDst == null)
                {
                    continue;
                }
                string dstPath = getInventory().getDBIndexPath(pCSDst);
                int indexDstPath = NLGUI.CDBManager.getInstance().getDbProp(dstPath + ":INDEX_IN_BAG").getValue16();

                // Already something in that slot?
                if (!allowReplace && indexDstPath > 0)
                {
                    continue;
                }

                // Does the source and destination are items ?
                if (pCSSrc.getType() == CtrlSheetInfo.SheetType_Item)
                {
                    if (pCSDst.getType() == CtrlSheetInfo.SheetType_Item)
                    {
                        // Right Slot ?
                        if (pCSDst.canDropItem(pCSSrc))
                        {
                            // Ok let us equip with this item
                            string srcPath = pCSSrc.getSheet();
                            equip(srcPath, dstPath);
                            return true;
                        }
                    }
                }
            }*/

            return false;
        }
    }
}
