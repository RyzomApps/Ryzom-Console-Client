using API;
using API.Commands;
using Client.Network.Action;
using System;
using System.Collections.Generic;

namespace Client.Commands
{
    public class Loot : CommandBase
    {
        public override string CmdName => "loot";

        public override string CmdUsage => "";

        public override string CmdDesc => "Loot Action";

        public override bool Run(IClient handler, string command, out string responseMsg, Dictionary<string, object> localVars)
        {
            responseMsg = "";
            if (handler is not RyzomClient ryzomClient)
                throw new Exception("Command handler is not a Ryzom client.");

            var target = ryzomClient.GetApiNetworkManager()?.GetApiEntityManager()?.GetApiUserEntity()?.GetSelection();

            if (target is null or Constants.InvalidSlot)
            {
                responseMsg = "Nothing selected.";
                return false;
            }

            ryzomClient.GetNetworkManager().PushPickup(target.Value, TargettingType.Lootable);

            return true;
        }
    }
}