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

        public override string Run(IClient handler, string command, Dictionary<string, object> localVars)
        {
            if (!(handler is RyzomClient ryzomClient))
                throw new Exception("Command handler is not a Ryzom client.");

            var args = GetArgs(command);

            if (args.Length != 1)
                return "Please inventory path (INVENTORY:HAND:0 OR INVENTORY:EQUIP:5).";

            var invPath = args[0].ToUpper().Trim();

            var inventoryManager = ryzomClient.GetInventoryManager();

            inventoryManager.UnEquip(invPath);

            return "";
        }

        public override IEnumerable<string> GetCmdAliases()
        {
            return new[] { "" };
        }
    }
}