using System;
using System.Collections.Generic;
using API;
using API.Commands;
using API.Entity;

namespace Client.Commands
{
    public class Target : CommandBase
    {
        public override string CmdName => "target";

        public override string CmdUsage => "<name>";

        public override string CmdDesc => "Finds the nearest entity whose name contains the given string.";

        public override string Run(IClient handler, string command, Dictionary<string, object> localVars)
        {
            if (!(handler is RyzomClient ryzomClient))
                throw new Exception("Command handler is not a Ryzom client.");

            var args = GetArgs(command);

            if (!(args.Length == 1 && byte.TryParse(args[0], out var slot)))
            {
                // Argument was not a slot id - so it has to be a name
                var entityName = "";

                if (args.Length != 0) entityName = string.Join(" ", args);

                // Try to get the entity with complete match
                var entity = ryzomClient.GetNetworkManager().GetEntityManager().GetEntityByName(entityName, false, true);

                if (entity == null)
                    return $"Could not find '{entityName}'.";

                slot = entity.Slot();
            }

            ryzomClient.GetNetworkManager().GetEntityManager().UserEntity.Selection(slot);
            ryzomClient.GetNetworkManager().GetEntityManager().UserEntity.SetTargetSlot(slot);

            return "";
        }

        public override IEnumerable<string> GetCmdAliases()
        {
            return new[] { "tar" };
        }
    }
}