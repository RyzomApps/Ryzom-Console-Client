// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

using API;
using API.Commands;
using System;
using System.Collections.Generic;

namespace Client.Commands
{
    public class Assist : CommandBase
    {

        public override CommandCategory CmdCategory => CommandCategory.Interaction;
        public override string CmdName => "assist";

        public override string CmdUsage => "[slot|name]";

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

            if (args.Length == 1 && byte.TryParse(args[0], out var slot))
            {
                if (entityManager.GetEntity(slot) == null)
                {
                    responseMsg = "Entity not found.";
                    return false;
                }

                user.Assist(slot);
                return true;
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