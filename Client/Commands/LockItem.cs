using API;
using API.Commands;
using System;
using System.Collections.Generic;

namespace Client.Commands
{
    public class LockItem : CommandBase
    {

        public override CommandCategory CmdCategory => CommandCategory.Inventory;
        public override string CmdName => "LockItem";

        public override string CmdUsage => "<inventory> <slot> <0|1>";

        public override string CmdDesc => "Lock or unlock an item in one of the player's inventories. A locked item cannot be sold, traded, dropped or destroyed. Use '0' to unlock and '1' to lock.";

        public override bool Run(IClient handler, string command, out string responseMsg, Dictionary<string, object> localVars)
        {
            responseMsg = "";
            if (handler is not RyzomClient ryzomClient)
                throw new Exception("Command handler is not a Ryzom client.");

            var args = GetArgs(command);

            // Check parameters and perform admin command
            if (args.Length == 3)
                return ryzomClient.PerformInternalCommand($"a lockItem 0 {args[0]} {args[1]} {args[2]}", out responseMsg);

            responseMsg = "Wrong argument count in the command.";
            return false;
        }
    }
}
