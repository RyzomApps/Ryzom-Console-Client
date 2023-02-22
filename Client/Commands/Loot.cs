using System;
using System.Collections.Generic;
using API;
using API.Commands;
using Client.Network.Action;

namespace Client.Commands
{
    public class Loot : CommandBase
    {
        public override string CmdName => "loot";

        public override string CmdUsage => "";

        public override string CmdDesc => "Loot Action";

        public override string Run(IClient handler, string command, Dictionary<string, object> localVars)
        {
            if (!(handler is RyzomClient ryzomClient))
                throw new Exception("Command handler is not a Ryzom client.");

            var target = ryzomClient.GetApiNetworkManager()?.GetApiEntityManager()?.GetApiUserEntity()?.GetSelection();

            if (!target.HasValue || target.Value == Constants.InvalidSlot)
                return "Nothing selected.";

            ryzomClient.GetNetworkManager().PushPickup(target.Value, TargettingType.Lootable);

            return "";
        }

        public override IEnumerable<string> GetCmdAliases()
        {
            return new[] { "" };
        }
    }
}