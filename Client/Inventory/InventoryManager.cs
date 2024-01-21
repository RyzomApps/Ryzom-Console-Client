///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

using Client.Database;

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
        private RyzomClient _client;

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
        //#define LOCAL_INVENTORY "LOCAL:INVENTORY"
        //#define SERVER_INVENTORY "SERVER:INVENTORY"

        // db path for all the inventories (without the SERVER: prefix)
        public readonly string[] InventoryDBs;
        public readonly uint[] InventoryIndexes;
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
    }
}
