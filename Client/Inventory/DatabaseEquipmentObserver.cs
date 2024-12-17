using Client.Interface;
using Client.Database;

namespace Client.Inventory
{
    /// <summary>
    /// Equipment observer
    /// </summary>
    public class DatabaseEquipObs : IPropertyObserver
    {
        private readonly RyzomClient _client;

        /// <summary>
        /// Constructor
        /// </summary>
        public DatabaseEquipObs(RyzomClient client)
        {
            _client = client;
        }

        /// <summary>
        /// Observer on DB equipment branch
        /// </summary>
        public override void Update(DatabaseNode node)
        {
            //CInterfaceManager pIM = CInterfaceManager.getInstance();
            var sTmp = node.GetFullName();

            if (!(node is DatabaseNodeLeaf pNL))
                return;

            if (sTmp.StartsWith("SERVER:INVENTORY:HAND"))
            {
                // Coming from hand
                sTmp = sTmp[22..];
                sTmp = sTmp[..sTmp.LastIndexOf(':')];
                var index = uint.Parse(sTmp);
                // update Hands.
                _client.GetInventoryManager().ServerHands[index] = pNL.GetValue16() - 1;
            }
            else if (sTmp.StartsWith("SERVER:INVENTORY:EQUIP"))
            {
                // Coming from equipement
                sTmp = sTmp[23..];
                sTmp = sTmp[..sTmp.LastIndexOf(':')];
                var index = uint.Parse(sTmp);
                // update Equips.
                _client.GetInventoryManager().ServerEquip[index] = pNL.GetValue16() - 1;
            }
            else if (sTmp.StartsWith("SERVER:INVENTORY:HOTBAR"))
            {
                // Coming from hand
                sTmp = sTmp[24..];
                sTmp = sTmp[..sTmp.LastIndexOf(':')];
                var index = uint.Parse(sTmp);
                // update Hotbar.
                _client.GetInventoryManager().ServerHotbar[index] = pNL.GetValue16() - 1;
            }

            // Remove Last reference and update database
            var oldVal = pNL.GetOldValue16() - 1;
            var newVal = pNL.GetValue16() - 1;

            if (oldVal != -1)
            {
                _client.GetInventoryManager().UnwearBagItem(oldVal);
            }

            if (newVal != -1)
            {
                _client.GetInventoryManager().WearBagItem(newVal);
                // ignored
            }
            else
            {
                // in some case left sheet is same than right sheet so don't clear it now (ex: 2 hands item, right hand exclusive) ignored
            }

            // Hands management ignored since there is no UI
        }
    }
}