using System;
using System.Collections.Generic;
using API;
using API.Commands;

namespace Client.Commands
{
    public class AutoEquip : CommandBase
    {
        public override string CmdName => "AutoEquip";

        public override string CmdUsage => "<index>";

        public override string CmdDesc => "Auto equip an item from the bag.";

        public override string Run(IClient handler, string command, Dictionary<string, object> localVars)
        {
            if (!(handler is RyzomClient ryzomClient))
                throw new Exception("Command handler is not a Ryzom client.");

            var args = GetArgs(command);

            if (args.Length != 1)
                return "Please specify the index of an item in the bag.";

            var itemIndex = int.Parse(args[0]);

            var inventoryManager = ryzomClient.GetInventoryManager();

            inventoryManager.AutoEquip(itemIndex, true);

            return "";
        }

        public override IEnumerable<string> GetCmdAliases()
        {
            return new[] { "" };
        }
    }
}