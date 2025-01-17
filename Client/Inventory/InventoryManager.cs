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
using API.Inventory;
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
    public class InventoryManager : IInventoryManager
    {
        private readonly RyzomClient _client;

        private const uint MaxTempinvEntries = 16;
        private const uint MaxBaginvEntries = 500;
        private const uint MaxHandinvEntries = 2;
        private const uint MaxEquipinvEntries = 19;
        private const uint MaxAnimalinvEntries = 500;
        private const uint MaxHotbarinvEntries = 5;
         
        private const int MaxPackAnimal = 3;
        private const int MaxMektoubMount = 1;
        private const int MaxOtherPet = 3;
         
        private const int MaxInventoryAnimal = (MaxPackAnimal + MaxMektoubMount + MaxOtherPet);

        // db path for the local inventory
        private const string Inventory = "INVENTORY";

        private enum Inventories : ushort
        {
            // TODO : remove handling, merge it with equipement
            Handling = 0,
            Temporary = 1,                  // 1
            Equipment = 2,                  // 2
            Hotbar,                         // 3
            Bag,                            // 4
            PetAnimal,                      // 5 Character can have 7 pack animal
            PetAnimal1 = PetAnimal,         // for toString => TInventory convertion
            PetAnimal2,
            PetAnimal3,
            PetAnimal4,
            PetAnimal5,
            PetAnimal6,
            PetAnimal7,
            MaxPetAnimal,                   // 12
            NumInventory = MaxPetAnimal,    // 12
            Undefined = NumInventory,       // 12

            Exchange,                       // 13  This is not a bug : exchange is a fake inventory
            ExchangeProposition,            // 14  and should not count in the number of inventory
                                            // same for botChat trading.
            Trading,                        // 15
            RewardSharing,                  // 16 fake inventory, not in database.xml. Used by the item info protocol only
            Guild,                          // 17 (warning: number stored in guild saved file)
            PlayerRoom,                     // 18
            NumAllInventory				    // warning: distinct from NUM_INVENTORY
        }

        // SERVER INVENTORY
        private ItemImage[] _serverBag = new ItemImage[MaxBaginvEntries];
        private ItemImage[] _serverTempInv = new ItemImage[MaxTempinvEntries];
        public int[] ServerHands = new int[MaxHandinvEntries];
        public int[] ServerEquip = new int[MaxEquipinvEntries];
        public int[] ServerHotbar = new int[MaxHotbarinvEntries];
        private readonly ItemImage[][] _serverPaInv = new ItemImage[MaxInventoryAnimal][];

        // Cache to know if bag is locked or not, because of item worn
        private readonly bool[] _bagItemEquipped = new bool[MaxBaginvEntries];

        // Equipment observer
        private readonly DatabaseEquipObs _dbEquipObs;

        // ***************************************************************************
        // db path for all the inventories (without the SERVER: prefix)

        public InventoryManager(RyzomClient client)
        {
            _client = client;
            _dbEquipObs = new DatabaseEquipObs(client);

            uint i;

            for (i = 0; i < MaxHandinvEntries; ++i)
            {
                ServerHands[i] = 0;
            }

            for (i = 0; i < MaxEquipinvEntries; ++i)
            {
                ServerEquip[i] = 0;
            }

            for (i = 0; i < MaxBaginvEntries; i++)
            {
                _bagItemEquipped[i] = false;
            }
        }

        public void Init()
        {
            // LOCAL DB is not implemented
            InitIndirection($"{Inventory}:HAND:", ref ServerHands, MaxHandinvEntries, true);
            InitIndirection($"{Inventory}:EQUIP:", ref ServerEquip, MaxEquipinvEntries, true);
            InitIndirection($"{Inventory}:HOTBAR:", ref ServerHotbar, MaxHotbarinvEntries, true);

            // SERVER DB
            InitItemArray($"{Inventory}:BAG", ref _serverBag, MaxBaginvEntries);
            InitItemArray($"{Inventory}:TEMP", ref _serverTempInv, MaxTempinvEntries);

            // Init Animals
            for (uint i = 0; i < MaxInventoryAnimal; i++)
            {
                _serverPaInv[i] = new ItemImage[MaxAnimalinvEntries];
                InitItemArray($"{Inventory}:PACK_ANIMAL{i}", ref _serverPaInv[i], MaxAnimalinvEntries);
            }

            // Drag'n'Drop is not implemented

            // ItemInfoObservers are not implemented
        }

        /// <summary>
        /// Init an array of items from a db branch
        /// </summary>
        private void InitItemArray(in string dbBranchName, ref ItemImage[] dest, uint numItems)
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
        /// Init array of int that represents indirection to the bag
        /// </summary>
        private void InitIndirection(string dbbranch, ref int[] indices, uint nbIndex, bool putObs)
        {
            for (uint i = 0; i < nbIndex; ++i)
            {
                var pNl = _client.GetDatabaseManager().GetServerNode(dbbranch + i + ":INDEX_IN_BAG");

                if (putObs)
                {
                    var textId = new TextId();
                    pNl.AddObserver(_dbEquipObs, textId);
                }
                if (pNl != null)
                {
                    indices[i] = pNl.GetValue32();
                }
            }
        }

        /// <summary>
        /// Called on impulse
        /// </summary>
        internal void OnUpdateEquipHands()
        {
            // update hands slots after initial BAG inventory has received
            var pNl = _client.GetDatabaseManager().GetServerNode($"{Inventory}:HAND:0:INDEX_IN_BAG", false);

            if (pNl != null && pNl.GetValue32() != 0)
            {
                _dbEquipObs.Update(pNl);
                _client.Plugins.OnInitEquipHands(0, pNl.GetValue32());
            }

            pNl = _client.GetDatabaseManager().GetServerNode($"{Inventory}:HAND:1:INDEX_IN_BAG", false);

            if (pNl == null || pNl.GetValue32() == 0)
                return;

            _dbEquipObs.Update(pNl);
            _client.Plugins.OnInitEquipHands(1, pNl.GetValue32());
        }

        /// <summary>
        /// Dump all inventory to a file.
        /// </summary>
        public void Write(string fileName)
        {
            var f = new StreamWriter(fileName, false);

            for (uint i = 0; i < MaxBaginvEntries; i++)
            {
                var sbi = GetBagItem(i);

                if (sbi == null || sbi.GetSheetId() == 0)
                    continue;

                var sheetId = _client.GetApiSheetIdFactory().SheetId(sbi.GetSheetId());
                var name = "";
                var family = "";

                if (sbi.GetNameId() != 0)
                    _client.GetStringManager().GetDynString(sbi.GetNameId(), out name, _client.GetNetworkManager());

                var sheet = _client.GetApiSheetManager().Get(sheetId);

                if (sheet is ItemSheet itemSheet)
                    family = itemSheet.Family.ToString();

                f.WriteLine($"{i}\t{sheetId.Name}\t{family}\t{name}\t{sbi.GetQuality()}\t{sbi.GetQuantity()}");
            }

            f.Close();
        }

        public IItemImage GetHandItem(uint index)
        {
            Debug.Assert(index < MaxHandinvEntries);
            return ServerHands[index] > 0 ? _serverBag[ServerHands[index]] : null;
        }

        public IItemImage GetEquipmentItem(uint index)
        {
            Debug.Assert(index < MaxEquipinvEntries);
            return ServerEquip[index] > 0 ? _serverBag[ServerEquip[index]] : null;
        }

        public IItemImage GetPackAnimalItem(uint beastIndex, uint index)
        {
            Debug.Assert(beastIndex < MaxInventoryAnimal);
            Debug.Assert(index < MaxAnimalinvEntries);
            return _serverPaInv[beastIndex][index];
        }

        public IItemImage GetBagItem(uint index)
        {
            Debug.Assert(index < MaxBaginvEntries);
            return _serverBag[index];
        }

        public void WearBagItem(int bagEntryIndex)
        {
            if (bagEntryIndex < 0 || bagEntryIndex >= (int)MaxBaginvEntries)
                return;

            _bagItemEquipped[bagEntryIndex] = true;
        }

        public void UnwearBagItem(int bagEntryIndex)
        {
            if (bagEntryIndex < 0 || bagEntryIndex >= (int)MaxBaginvEntries)
                return;

            _bagItemEquipped[bagEntryIndex] = false;
        }

        /// <summary>
        /// Equip a bag item
        /// </summary>
        /// <param name="bagPath">INVENTORY:BAG:165</param>
        /// <param name="invPath">INVENTORY:HAND:0 OR INVENTORY:EQUIP:5</param>
        public void Equip(in string bagPath, in string invPath)
        {
            if (bagPath.Length == 0 || invPath.Length == 0)
                return;

            // Get inventory and slot
            var sIndexInBag = bagPath[(bagPath.LastIndexOf(':') + 1)..];
            if (!ushort.TryParse(sIndexInBag, out var indexInBag))
            {
                _client.Log.Error("Could not parse bag index.");
                return;
            }

            var inventory = (ushort)Inventories.Undefined;
            ushort invSlot = 0xffff;

            if (invPath.StartsWith("INVENTORY:HAND", StringComparison.InvariantCultureIgnoreCase))
            {
                inventory = (ushort)Inventories.Handling;
                if (!ushort.TryParse(invPath[15..], out invSlot))
                {
                    _client.Log.Error("Could not parse slot.");
                    return;
                }
            }
            else if (invPath.StartsWith("INVENTORY:EQUIP", StringComparison.InvariantCultureIgnoreCase))
            {
                inventory = (ushort)Inventories.Equipment;
                if (!ushort.TryParse(invPath[16..], out invSlot))
                {
                    _client.Log.Error("Could not parse slot.");
                    return;
                }
            }
            else if (invPath.StartsWith("INVENTORY:HOTBAR", StringComparison.InvariantCultureIgnoreCase))
            {
                inventory = (ushort)Inventories.Hotbar;
                if (!ushort.TryParse(invPath[17..], out invSlot))
                {
                    _client.Log.Error("Could not parse slot.");
                    return;
                }
            }

            // update the equip DB pointer
            _client.GetDatabaseManager().GetServerNode($"SERVER:{invPath}:INDEX_IN_BAG").SetValue16((short)(indexInBag + 1));

            // Send message to the server
            if (inventory != (short)Inventories.Undefined)
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
                }
                else
                {
                    _client.Log.Error($"Don't know message name {sMsg}");
                }
            }
            else
            {
                _client.Log.Error("Inventory is undefined.");
            }
        }

        /// <summary>
        /// Unequip an item
        /// </summary>
        /// <param name="invPath">INVENTORY:HAND:0 OR INVENTORY:EQUIP:5</param>
        internal void UnEquip(string invPath)
        {
            if (invPath.Length == 0)
                return;

            // Get inventory and slot
            var inventory = (ushort)Inventories.Undefined;
            ushort invSlot = 0xffff;

            if (invPath.StartsWith("INVENTORY:HAND", StringComparison.InvariantCultureIgnoreCase))
            {
                inventory = (ushort)Inventories.Handling;
                if (!ushort.TryParse(invPath[15..], out invSlot))
                {
                    _client.Log.Error("Could not parse slot.");
                    return;
                }
            }
            else if (invPath.StartsWith("INVENTORY:EQUIP", StringComparison.InvariantCultureIgnoreCase))
            {
                inventory = (ushort)Inventories.Equipment;
                if (!ushort.TryParse(invPath[16..], out invSlot))
                {
                    _client.Log.Error("Could not parse slot.");
                    return;
                }
            }
            else if (invPath.StartsWith("INVENTORY:HOTBAR", StringComparison.InvariantCultureIgnoreCase))
            {
                inventory = (ushort)Inventories.Hotbar;
                if (!ushort.TryParse(invPath[17..], out invSlot))
                {
                    _client.Log.Error("Could not parse slot.");
                    return;
                }
            }

            // Send message to the server
            if (inventory != (ushort)Inventories.Undefined)
            {
                var @out = new BitMemoryStream();
                const string sMsg = "ITEM:UNEQUIP";
                
                if (_client.GetNetworkManager().GetMessageHeaderManager().PushNameToStream(sMsg, @out))
                {
                    // Fill the message (equipped inventory, equipped inventory slot)
                    @out.Serial(ref inventory);
                    @out.Serial(ref invSlot);
                    _client.GetNetworkManager().Push(@out);
                }
                else
                {
                    _client.Log.Error($"Don't know message name {sMsg}");
                }
            }
            else
            {
                _client.Log.Error("Inventory is undefined.");
            }
        }
    }
}
