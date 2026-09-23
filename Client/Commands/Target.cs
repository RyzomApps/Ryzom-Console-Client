using API;
using API.Commands;
using System;
using System.Collections.Generic;

namespace Client.Commands
{
    public class Target : CommandBase
    {
        public override string CmdName => "target";

        public override string CmdUsage => "<name>";

        public override string CmdDesc => "Finds the nearest entity whose name contains the given string.";

        public override bool Run(IClient handler, string command, out string responseMsg, Dictionary<string, object> localVars)
        {
            responseMsg = "";
            if (handler is not RyzomClient ryzomClient)
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
                {
                    responseMsg = $"Could not find '{entityName}'.";
                    return true;
                }

                slot = entity.Slot();
            }

            ryzomClient.GetNetworkManager().GetEntityManager().UserEntity.SetSelection(slot);
            ryzomClient.GetNetworkManager().GetEntityManager().UserEntity.SetTargetSlot(slot);

            return true;
        }

        public override IEnumerable<string> GetCmdAliases()
        {
            return ["tar"];
        }
    }
}