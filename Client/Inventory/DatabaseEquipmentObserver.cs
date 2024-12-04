using System;
using Client.Interface;
using System.Collections.Generic;
using Client.Database;
using Client.Sheet;

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

            if (!(node is DatabaseNodeLeaf pNl))
            {
                return;
            }

            // Set database for wearing the right item
            var vCs = new List<DatabaseCtrlSheet>();

            if (sTmp[20..].Equals("SERVER:INVENTORY:HAND", StringComparison.InvariantCultureIgnoreCase))
            {
                // Coming from hand
                sTmp = sTmp.Substring(21, sTmp.Length);
                sTmp = sTmp[..sTmp.LastIndexOf(':')];
                var index = int.Parse(sTmp);

                // local hands not implemented

                // update Hands.
                _client.GetInventoryManager().ServerHands[index] = pNl.GetValue16();
            }
            else if (sTmp.StartsWith("SERVER:INVENTORY:EQUIP", StringComparison.InvariantCultureIgnoreCase))
            {
                // Coming from equipement
                sTmp = sTmp.Substring(22, sTmp.Length);
                sTmp = sTmp[..sTmp.LastIndexOf(':')];
                var index = int.Parse(sTmp);

                // local equip not implemented

                // update Equips.
                _client.GetInventoryManager().ServerEquip[index] = pNl.GetValue16();
            }
            else if (sTmp.StartsWith("SERVER:INVENTORY:HOTBAR", StringComparison.InvariantCultureIgnoreCase))
            {
                // Coming from hand
                sTmp = sTmp[23..sTmp.Length];
                sTmp = sTmp[..sTmp.LastIndexOf(':')];
                var index = int.Parse(sTmp);

                // local hotbar not implemented

                // update Hotbar.
                _client.GetInventoryManager().ServerHotbar[index] = pNl.GetValue16();
            }

            if (vCs.Count == 0)
            {
                return;
            }

            // Remove Last reference and update database
            var oldVal = pNl.GetOldValue16() - 1;
            var newVal = pNl.GetValue16() - 1;

            if (oldVal != -1)
            {
                _client.GetInventoryManager().UnwearBagItem(oldVal);
            }

            if (newVal != -1)
            {
                _client.GetInventoryManager().WearBagItem(newVal);

                // ControlSheets are not implemented
            }
            else
            {
                // in some case left sheet is same than right sheet so don't clear it now (ex: 2 hands item, right hand exclusive) not implemented
            }

            // Hands management in not implemented
        }
    }
}