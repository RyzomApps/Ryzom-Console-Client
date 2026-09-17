using System;
using System.Collections.Generic;
using API;
using API.Commands;

namespace Client.Commands
{
    public class Assist : CommandBase
    {
        public override string CmdName => "assist";

        public override string CmdUsage => "[name]";

        public override string CmdDesc => "Targets the target of the targeted entity.";

        public override bool Run(IClient handler, string command, out string responseMsg, Dictionary<string, object> localVars)
        {
            responseMsg = "";
            if (handler is not RyzomClient ryzomClient)
                throw new Exception("Command handler is not a Ryzom client.");

            var args = GetArgs(command);

            var entityName = "";

            if (args.Length != 0) entityName = string.Join(" ", args);

            var entityManager = ryzomClient.GetNetworkManager().GetEntityManager();

            var user = entityManager.UserEntity;

            if (user == null)
            {
                responseMsg = "User not found.";
                return false;
            }

            var entity = entityName != string.Empty
                ? entityManager.GetEntityByName(entityName, false, false)
                : entityManager.GetEntity(user.TargetSlot());

            if (entity == null)
            {
                responseMsg = "Entity not found.";
                return false;
            }

            // Select the entity
            user.Assist(entity.Slot());
            return true;
        }

        public override IEnumerable<string> GetCmdAliases()
        {
            return ["as"];
        }
    }
}