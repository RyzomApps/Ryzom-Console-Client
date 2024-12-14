using System;
using System.Collections.Generic;
using API;
using API.Commands;

namespace Client.Commands
{
    public class Equip : CommandBase
    {
        public override string CmdName => "Equip";

        public override string CmdUsage => "<bagPath> <invPath>";

        public override string CmdDesc => "Equip an item from the bag.";

        public override string Run(IClient handler, string command, Dictionary<string, object> localVars)
        {
            if (!(handler is RyzomClient ryzomClient))
                throw new Exception("Command handler is not a Ryzom client.");

            var args = GetArgs(command);

            if (args.Length != 2)
                return "Please specify a bag path (INVENTORY:BAG:165) and an inventory path (INVENTORY:HAND:0 OR INVENTORY:EQUIP:5).";

            var bagPath = args[0].ToUpper().Trim();
            var invPath = args[1].ToUpper().Trim();

            var inventoryManager = ryzomClient.GetInventoryManager();

            inventoryManager.Equip(bagPath, invPath);

            return "";
        }

        public override IEnumerable<string> GetCmdAliases()
        {
            return new[] { "" };
        }
    }
}