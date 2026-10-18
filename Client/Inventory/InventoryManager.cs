///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

using API.Inventory;
using Client.Database;
using Client.Sheet;
using Client.Stream;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

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

            // Indirection arrays hold 0-based bag child indices (-1 = empty).
            // The server DB stores INDEX_IN_BAG = child + 1 (0 = empty), see
            // DatabaseEquipObs which applies the same -1 conversion.
            for (i = 0; i < MaxHandinvEntries; ++i)
            {
                ServerHands[i] = -1;
            }

            for (i = 0; i < MaxEquipinvEntries; ++i)
            {
                ServerEquip[i] = -1;
            }

            for (i = 0; i < MaxHotbarinvEntries; ++i)
            {
                ServerHotbar[i] = -1;
            }

            for (i = 0; i < MaxBaginvEntries; i++)
            {
                _bagItemEquipped[i] = false;
            }
        }

        public void Init()
        {
            // Without the database (UseDatabase = false) there is nothing to bind to.
            if (_client.GetDatabaseManager() == null)
            {
                _client.Log.Warn("InventoryManager.Init skipped: database disabled (UseDatabase = false).");
                return;
            }

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
                if (branch.GetNode((ushort)k) is not DatabaseNodeBranch itemBranch)
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
                var pNl = _client.GetDatabaseManager().GetServerNode($"{dbbranch}{i}:INDEX_IN_BAG");

                if (pNl != null)
                {
                    if (putObs)
                    {
                        var textId = new TextId();
                        pNl.AddObserver(_dbEquipObs, textId);
                    }
                    // DB stores INDEX_IN_BAG = bag child + 1 (0 = empty);
                    // store the 0-based child index (-1 = empty), consistent
                    // with DatabaseEquipObs.Update which does the same -1.
                    indices[i] = pNl.GetValue32() - 1;
                }
            }
        }

        /// <summary>
        /// Called on impulse
        /// </summary>
        internal void OnUpdateEquipHands()
        {
            if (_client.GetDatabaseManager() == null)
                return;

            // update hands slots after initial BAG inventory has received
            var pNl = _client.GetDatabaseManager().GetServerNode($"{Inventory}:HAND:0:INDEX_IN_BAG", false);

            if (pNl != null && pNl.GetValue32() != 0)
            {
                _dbEquipObs.Update(pNl);
                _client.Plugins.OnInitEquipHands(0, pNl.GetValue32() - 1);
            }

            pNl = _client.GetDatabaseManager().GetServerNode($"{Inventory}:HAND:1:INDEX_IN_BAG", false);

            if (pNl == null || pNl.GetValue32() == 0)
                return;

            _dbEquipObs.Update(pNl);
            _client.Plugins.OnInitEquipHands(1, pNl.GetValue32() - 1);
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
            return ServerHands[index] >= 0 ? _serverBag[ServerHands[index]] : null;
        }

        public IItemImage GetEquipmentItem(uint index)
        {
            Debug.Assert(index < MaxEquipinvEntries);
            return ServerEquip[index] >= 0 ? _serverBag[ServerEquip[index]] : null;
        }

        public IItemImage GetPackAnimalItem(uint beastIndex, uint index)
        {
            Debug.Assert(beastIndex < MaxInventoryAnimal);
            Debug.Assert(index < MaxAnimalinvEntries);
            return _serverPaInv[beastIndex][index];
        }

        /// <summary>
        /// Use/consume the item in the given bag slot (sends ITEM:USE_ITEM, u16 bag slot).
        /// </summary>
        public bool Use(uint bagSlot, out string error)
        {
            error = "";

            if (bagSlot >= MaxBaginvEntries)
            {
                error = "invalid bag slot";
                return false;
            }

            var item = _serverBag[bagSlot];
            if (item == null || item.GetSheetId() == 0)
            {
                error = "item info not received";
                return false;
            }

            var slot = (ushort)bagSlot;
            var @out = new BitMemoryStream();
            const string sMsg = "ITEM:USE_ITEM";

            if (!_client.GetNetworkManager().GetMessageHeaderManager().PushNameToStream(sMsg, @out))
            {
                _client.Log.Error($"Don't know message name {sMsg}");
                error = "message not known";
                return false;
            }

            // Fill the message (bag slot)
            @out.Serial(ref slot);
            _client.GetNetworkManager().Push(@out);

            return true;
        }

        public IItemImage GetBagItem(uint index)
        {
            Debug.Assert(index < MaxBaginvEntries);
            return _serverBag[index];
        }

        /// <inheritdoc/>
        public List<IBagEntry> GetBagEntries()
        {
            var result = new List<IBagEntry>();

            var bagBranch = _client.GetDatabaseManager().GetServerBranch($"{Inventory}:BAG");

            for (uint i = 0; i < MaxBaginvEntries; i++)
            {
                var itemBranch = bagBranch?.GetNode((ushort)i) as DatabaseNodeBranch;
                var item = _serverBag[i];

                // Invalidate the cache when the branch is gone or has been
                // removed and rebuilt (e.g. after selling an item): otherwise
                // the stale image would report a ghost item.
                if (item != null && (itemBranch == null || !item.BelongsTo(bagBranch)))
                {
                    item = null;
                    _serverBag[i] = null;
                }

                if (item == null && itemBranch != null)
                {
                    // The item may have been received after the inventory was
                    // initialized (e.g. minimal config without sheets where the
                    // bag branch only appears later): build it now from the
                    // server database so live updates continue to flow.
                    item = new ItemImage();
                    item.Build(itemBranch);
                    _serverBag[i] = item;
                }

                if (item == null || item.GetSheetId() == 0)
                {
                    continue;
                }

                result.Add(new BagEntry(i, item));
            }

            return result;
        }

        /// <summary>
        /// Enumerate the items of any server inventory branch, e.g.
        /// INVENTORY:TEMP, INVENTORY:PACK_ANIMAL0..3, INVENTORY:SHARE,
        /// INVENTORY:GUILD, INVENTORY:ROOM. Items are built lazily from the
        /// server database, like GetBagEntries does.
        /// </summary>
        public List<IBagEntry> GetInventoryEntries(string dbPath)
        {
            var result = new List<IBagEntry>();

            if (_client.GetDatabaseManager() == null)
                return result;

            var branch = _client.GetDatabaseManager().GetServerBranch(dbPath);
            if (branch == null)
                return result;

            for (uint i = 0; i < branch.NodeCount(); i++)
            {
                if (branch.GetNode((ushort)i) is not DatabaseNodeBranch itemBranch)
                    continue;

                var item = new ItemImage();
                item.Build(itemBranch);

                if (item.GetSheetId() == 0)
                    continue;

                result.Add(new BagEntry(i, item));
            }

            return result;
        }

        /// <summary>
        /// Resolve one indirection slot (hand or hotbar) to the bag item it
        /// references. A negative index (or out of range) means the slot is
        /// empty. The referenced bag item is built lazily, like GetBagEntries does.
        /// </summary>
        private bool TryResolveSlotItem(int[] indices, uint slot, out ItemImage item)
        {
            item = null;

            if (slot >= (uint)indices.Length)
                return false;

            var bagIndex = indices[slot];

            if (bagIndex < 0 || bagIndex >= (int)MaxBaginvEntries)
                return false;

            var branch = _client.GetDatabaseManager().GetServerBranch($"{Inventory}:BAG");
            var itemBranch = branch?.GetNode((ushort)bagIndex) as DatabaseNodeBranch;
            var existing = _serverBag[(uint)bagIndex];
            item = existing;

            // Invalidate the cache when the branch is gone or has been
            // removed and rebuilt (e.g. after selling an item): otherwise
            // the stale image would report a ghost item.
            if (existing != null && (itemBranch == null || !existing.BelongsTo(branch)))
            {
                existing = null;
                item = null;
                _serverBag[(uint)bagIndex] = null;
            }

            if (item == null && itemBranch != null)
            {
                existing = new ItemImage();
                existing.Build(itemBranch);
                _serverBag[(uint)bagIndex] = existing;
                item = existing;
            }

            return item != null && item.GetSheetId() != 0;
        }

        /// <summary>
        /// Enumerate non-empty slots of an indirection branch (hands or hotbar).
        /// </summary>
        private List<IBagEntry> GetIndirectionEntries(int[] indices)
        {
            var result = new List<IBagEntry>();

            for (uint i = 0; i < (uint)indices.Length; i++)
            {
                if (indices[i] >= 0 && TryResolveSlotItem(indices, i, out var item))
                    result.Add(new BagEntry((uint)indices[i], item));
            }

            return result;
        }

        /// <inheritdoc/>
        public List<IBagEntry> GetHandEntries()
        {
            return GetIndirectionEntries(ServerHands);
        }

        /// <inheritdoc/>
        public List<IBagEntry> GetHotbarEntries()
        {
            return GetIndirectionEntries(ServerHotbar);
        }

        /// <summary>
        /// One non-empty bag slot.
        /// </summary>
        private sealed class BagEntry : IBagEntry
        {
            public uint Index { get; }
            public IItemImage Item { get; }

            public BagEntry(uint index, IItemImage item)
            {
                Index = index;
                Item = item;
            }
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
