using API;
using API.Commands;
using System;
using System.Collections.Generic;

namespace Client.Commands
{
    public class Equip : CommandBase
    {

        public override CommandCategory CmdCategory => CommandCategory.Inventory;
        public override string CmdName => "Equip";

        public override string CmdUsage => "<bagPath> <invPath>";

        public override string CmdDesc => "Equip an item from the bag.";

        public override bool Run(IClient handler, string command, out string responseMsg, Dictionary<string, object> localVars)
        {
            responseMsg = "";
            if (handler is not RyzomClient ryzomClient)
                throw new Exception("Command handler is not a Ryzom client.");

            var args = GetArgs(command);

            if (args.Length != 2)
            {
                responseMsg = "Please specify a bag path (INVENTORY:BAG:165) and a target path (INVENTORY:EQUIP:5, INVENTORY:HAND:0, or INVENTORY:HOTBAR:0-4 for pocket).";
                return false;
            }

            var bagPath = args[0].ToUpper().Trim();
            var invPath = args[1].ToUpper().Trim();

            var inventoryManager = ryzomClient.GetInventoryManager();

            inventoryManager.Equip(bagPath, invPath);

            responseMsg = $"Equip: {bagPath} -> {invPath}\nNote: target can be an equipment slot (INVENTORY:EQUIP:n), the hand (INVENTORY:HAND:0|1) or a pocket slot (INVENTORY:HOTBAR:0-4).";
            return true;
        }
    }
}