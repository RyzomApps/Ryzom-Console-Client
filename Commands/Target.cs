using RCC.Commands.Internal;
using System.Collections.Generic;

namespace RCC.Commands
{
    public class Target : CommandBase
    {
        public override string CmdName => "target";
        public override string CmdUsage => "<name>";
        public override string CmdDesc => "Finds the nearest entity whose name contains the given string.";

        public override string Run(RyzomClient handler, string command, Dictionary<string, object> localVars)
        {
            var args = GetArgs(command);

            var entityName = "";

            if (args.Length != 0)
            {
                entityName = string.Join(" ", args);
            }

            // Try to get the entity with complete match first
            var entity = handler.GetNetworkManager().GetEntityManager().GetEntityByName(entityName, false, true);

            if (entity == null)
            {
                handler.GetLogger().Warn($"Could not find '{entityName}'.");
                return "";
            }

            handler.GetNetworkManager().GetEntityManager().UserEntity.Selection(entity.Slot(), handler);
            handler.GetNetworkManager().GetEntityManager().UserEntity.SetTargetSlot(entity.Slot());

            return "";
        }

        public override IEnumerable<string> GetCmdAliases()
        {
            return new[] { "tar" };
        }
    }
}