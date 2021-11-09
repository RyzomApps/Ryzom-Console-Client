using System.Collections.Generic;
using Client.Commands.Internal;

namespace Client.Commands
{
    public class Assist : CommandBase
    {
        public override IEnumerable<string> GetCmdAliases() { return new string[] { "as" }; }
        public override string CmdName => "assist";
        public override string CmdUsage => "[<name>]";
        public override string CmdDesc => "Targets the target of the targeted entity.";

        public override string Run(RyzomClient handler, string command, Dictionary<string, object> localVars)
        {
            var args = GetArgs(command);

            string entityName = "";

            if (args.Length != 0)
            {
                entityName = string.Join(" ", args);
            }

            var entityManager = handler.GetNetworkManager().GetEntityManager();

            Entity.Entity entity;
            var user = entityManager.UserEntity;
            if (user == null)
                return "";

            if (entityName != string.Empty)
            {
                entity = entityManager.GetEntityByName(entityName, false, false);
            }
            else
            {
                entity = entityManager.GetEntity(user.TargetSlot());
            }

            if (entity != null)
            {
                // Select the entity
                user.Assist(entity.Slot(), handler);
            }
            else
            {
                handler.GetLogger().Warn($"Entity not found.");
            }

            return "";
        }
    }
}