using System;
using System.Collections.Generic;
using API;
using API.Commands;

namespace Client.Commands
{
    public class UnEquip : CommandBase
    {
        public override string CmdName => "UnEquip";

        public override string CmdUsage => "<invPath>";

        public override string CmdDesc => "Unequip an item.";

        public override bool Run(IClient handler, string command, out string responseMsg, Dictionary<string, object> localVars)
        {
            responseMsg = "";
            if (handler is not RyzomClient ryzomClient)
                throw new Exception("Command handler is not a Ryzom client.");

            var args = GetArgs(command);

            if (args.Length != 1)
            {
                responseMsg = "Please inventory path (INVENTORY:HAND:0 OR INVENTORY:EQUIP:5).";
                return false;
            }

            var invPath = args[0].ToUpper().Trim();

            var inventoryManager = ryzomClient.GetInventoryManager();

            inventoryManager.UnEquip(invPath);

            return true;
        }
    }
}