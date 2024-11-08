///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

using System;
using Client.Database;
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
        const uint MAX_GUILDINV_ENTRIES = 1000;
        const uint MAX_ROOMINV_ENTRIES = 1000;

        // This is the personal player inventory max (bag and animal)
        const uint MAX_PLAYER_INV_ENTRIES = 500;

        // db path for the local inventory
        public const string LOCAL_INVENTORY = "LOCAL:INVENTORY";
        public const string SERVER_INVENTORY = "SERVER:INVENTORY";

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

        public readonly uint NumInventories = new uint();

        // LOCAL INVENTORY
        private ItemImage[] Bag = new ItemImage[MAX_BAGINV_ENTRIES];
        private ItemImage[] TempInv = new ItemImage[MAX_TEMPINV_ENTRIES];
        private int[] Hands = new int[MAX_HANDINV_ENTRIES];
        //private DatabaseCtrlSheet[] UIHands = new DatabaseCtrlSheet[MAX_HANDINV_ENTRIES];
        private int[] Equip = new int[MAX_EQUIPINV_ENTRIES];

        //private DatabaseCtrlSheet[] UIEquip = new DatabaseCtrlSheet[MAX_EQUIPINV_ENTRIES];
        //private DatabaseCtrlSheet[] UIEquip2 = new DatabaseCtrlSheet[MAX_EQUIPINV_ENTRIES];
        private DatabaseNodeLeaf Money;
        //private ItemImage[,] PAInv = new ItemImage[MAX_INVENTORY_ANIMAL, MAX_ANIMALINV_ENTRIES];

        // SERVER INVENTORY
        private ItemImage[] ServerBag = new ItemImage[MAX_BAGINV_ENTRIES];
        private ItemImage[] ServerTempInv = new ItemImage[MAX_TEMPINV_ENTRIES];
        private int[] ServerHands = new int[MAX_HANDINV_ENTRIES];
        private int[] ServerEquip = new int[MAX_EQUIPINV_ENTRIES];
        private DatabaseNodeLeaf ServerMoney;
        //private ItemImage[,] ServerPAInv = Array.Empty<ItemImage>(MAX_INVENTORY_ANIMAL, MAX_ANIMALINV_ENTRIES);

        // Drag'n'Drop
        //private TFrom DNDFrom = new TFrom();
        //private DatabaseCtrlSheet DNDCurrentItem;

        //private SortedDictionary<uint, ClientItemInfo> _ItemInfoMap = new SortedDictionary<uint, ClientItemInfo>();
        //private LinkedList<ItemInfoWaiter> _ItemInfoWaiters = new LinkedList<ItemInfoWaiter>();

        // Cache to know if bag is locked or not, because of item worn
        private bool[] BagItemEquipped = new bool[MAX_BAGINV_ENTRIES];

        // ***************************************************************************
        // db path for all the inventories (without the SERVER: prefix)

        public InventoryManager(RyzomClient client)
        {
            _client = client;

            Money = null;
            ServerMoney = null;

            uint i;

            for (i = 0; i < MAX_HANDINV_ENTRIES; ++i)
            {
                Hands[i] = ServerHands[i] = 0;
                //UIHands[i] = null;
            }

            for (i = 0; i < MAX_EQUIPINV_ENTRIES; ++i)
            {
                Equip[i] = ServerEquip[i] = 0;
                //UIEquip[i] = null;
                //UIEquip2[i] = null;
            }

            for (i = 0; i < MAX_BAGINV_ENTRIES; i++)
            {
                BagItemEquipped[i] = false;
            }

            //Debug.Assert(NumInventories == InventoryIndexes.Length);
        }

        internal void OnUpdateEquipHands()
        {
            // TODO
        }

        internal void SortBag()
        {
            // TODO
        }

        public void equip(in string bagPath, in string invPath)
        {
            //if (isSwimming() || isStunned() || isDead() || isRiding())
            //{
            //	return;
            //}

            var pIM = _client.GetInterfaceManager();

            if (bagPath.Length == 0 || invPath.Length == 0)
            {
                return;
            }

            // Get inventory and slot
            var sIndexInBag = bagPath.Substring(bagPath.LastIndexOf(':') + 1, bagPath.Length);
            ushort.TryParse(sIndexInBag, out var indexInBag);

            var inventory = (ushort)INVENTORIES.UNDEFINED;
            ushort invSlot = 0xffff;

            if (invPath.StartsWith("LOCAL:INVENTORY:HAND", StringComparison.InvariantCultureIgnoreCase))
            {
                inventory = (ushort)INVENTORIES.handling;
                ushort.TryParse(invPath.Substring(21, invPath.Length), out invSlot);
            }
            else if (invPath.StartsWith("LOCAL:INVENTORY:EQUIP", StringComparison.InvariantCultureIgnoreCase))
            {
                inventory = (ushort)INVENTORIES.equipment;
                ushort.TryParse(invPath.Substring(22, invPath.Length), out invSlot);
            }

            // Hands management : check if we have to unequip left hand because of incompatibility with right hand item
            var oldRightIndexInBag = _client.GetDatabaseManager().GetDbProp(invPath + ":INDEX_IN_BAG").GetValue16();

            if (inventory == (ushort)INVENTORIES.handling && invSlot == 0)
            {
                // TODO DatabaseCtrlSheet pCSLeftHand = CWidgetManager.getInstance().getElementFromId(CTRL_HAND_LEFT) as DatabaseCtrlSheet;
                object pCSLeftHand = null;

                if (pCSLeftHand == null)
                {
                    return;
                }

                // get sheet of left item
                uint leftSheet = 0;
                var pNL = _client.GetDatabaseManager().GetDbProp(LOCAL_INVENTORY + ":HAND:1:INDEX_IN_BAG", false);

                if (pNL == null)
                {
                    return;
                }
                if (pNL.GetValue32() > 0)
                {
                    var pNL2 = _client.GetDatabaseManager().GetDbProp(LOCAL_INVENTORY + ":BAG:" + (pNL.GetValue32() - 1) + ":SHEET", false);
                    if (pNL2 == null)
                    {
                        return;
                    }
                    leftSheet = (uint)pNL2.GetValue32();
                }

                // get sheet of previous right hand item
                uint lastRightSheet = 0;
                if (oldRightIndexInBag > 0)
                {
                    pNL = _client.GetDatabaseManager().GetDbProp(LOCAL_INVENTORY + ":BAG:" + (oldRightIndexInBag - 1) + ":SHEET", false);
                    if (pNL == null)
                    {
                        return;
                    }
                    lastRightSheet = (uint)pNL.GetValue32();
                }

                // get sheet of new right hand item
                uint rightSheet = 0;
                if (indexInBag + 1 > 0)
                {
                    pNL = _client.GetDatabaseManager().GetDbProp(LOCAL_INVENTORY + ":BAG:" + (indexInBag) + ":SHEET", false);
                    if (pNL == null)
                    {
                        return;
                    }
                    rightSheet = (uint)pNL.GetValue32();
                }

                //// TODO If incompatible -> remove
                //if (!GetInventory().IsLeftHandItemCompatibleWithRightHandItem(leftSheet, rightSheet, lastRightSheet))
                //{
                //    GetInventory().Unequip(LOCAL_INVENTORY+ ":HAND:1");
                //}
            }

            // update the equip DB pointer
            _client.GetDatabaseManager().GetDbProp(invPath + ":INDEX_IN_BAG").SetValue16((short)(indexInBag + 1));

            //// TODO Yoyo add: when the user equip an item, the action are invalid during some time
            //if (indexInBag < MAX_BAGINV_ENTRIES)
            //{
            //    ItemSheet pIS = _client.GetSheetManager().Get(_client.GetSheetIdFactory().SheetId(GetBagItem(indexInBag).getSheetID())) as ItemSheet;
            //    if (pIS != null)
            //    {
            //        PhraseManager pPM = PhraseManager.getInstance();
            //        pPM.setEquipInvalidation(NetMngr.getCurrentServerTick(), pIS.EquipTime);
            //    }
            //}

            //// TODO Update trade window if any
            //if ((BotChatPageAll != null) && (BotChatPageAll.Trade != null))
            //{
            //    BotChatPageAll.Trade.invalidateCoords();
            //}

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

                    // TODO pIM.IncLocalSyncActionCounter();

                    //nlinfo("impulseCallBack : %s %d %d %d sent", sMsg.c_str(), inventory, invSlot, indexInBag);
                }
                else
                {
                    _client.Log.Warn($"Don't know message name {sMsg}");
                }
            }
        }

    }
}
