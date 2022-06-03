using System;
using System.Collections.Generic;
using API;
using API.Commands;

namespace Client.Commands
{
    public class Assist : CommandBase
    {
        public override string CmdName => "assist";

        public override string CmdUsage => "[<name>]";

        public override string CmdDesc => "Targets the target of the targeted entity.";

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

            var entityManager = ryzomClient.GetNetworkManager().GetEntityManager();

            var user = entityManager.UserEntity;

            if (user == null)
                return "";

            var entity = entityName != string.Empty ? entityManager.GetEntityByName(entityName, false, false) : entityManager.GetEntity(user.TargetSlot());

            if (entity != null)
            {
                // Select the entity
                user.Assist(entity.Slot());
            }
            else
            {
                handler.GetLogger().Warn("Entity not found.");
            }

            return "";
        }

        public override IEnumerable<string> GetCmdAliases() { return new[] { "as" }; }
    }
}