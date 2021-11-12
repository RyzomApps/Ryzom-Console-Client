using System;
using System.Collections.Generic;
using API;
using API.Commands;

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

            var entityName = "";

            if (args.Length != 0)
            {
                entityName = string.Join(" ", args);
            }

            // Try to get the entity with complete match first
            var entity = ryzomClient.GetNetworkManager().GetEntityManager().GetEntityByName(entityName, false, true);

            if (entity == null)
            {
                handler.GetLogger().Warn($"Could not find '{entityName}'.");
                return "";
            }

            ryzomClient.GetNetworkManager().GetEntityManager().UserEntity.Selection(entity.Slot(), ryzomClient);
            ryzomClient.GetNetworkManager().GetEntityManager().UserEntity.SetTargetSlot(entity.Slot());

            return "";
        }

        public override IEnumerable<string> GetCmdAliases()
        {
            return new[] { "tar" };
        }
    }
}