using System;
using System.Collections.Generic;
using API;
using API.Commands;
using Client.Network.Action;

namespace Client.Commands
{
    public class Quarter : CommandBase
    {
        public override string CmdName => "quarter";

        public override string CmdUsage => "";

        public override string CmdDesc => "Quartering Action.";

        public override string Run(IClient handler, string command, Dictionary<string, object> localVars)
        {
            if (!(handler is RyzomClient ryzomClient))
                throw new Exception("Command handler is not a Ryzom client.");

            var target = ryzomClient.GetApiNetworkManager()?.GetApiEntityManager()?.GetApiUserEntity()?.TargetSlot();

            if (target == null)
                return "Nothing selected.";

            ryzomClient.GetNetworkManager().PushPickup((byte)target, TargettingType.Harvestable);

            return "";
        }

        public override IEnumerable<string> GetCmdAliases()
        {
            return new[] { "loot" };
        }
    }
}