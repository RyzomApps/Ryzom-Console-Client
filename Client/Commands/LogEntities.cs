using System;
using System.Collections.Generic;
using API;
using API.Commands;

namespace Client.Commands
{
    public class LogEntities : CommandBase
    {
        public override string CmdName => "logEntities";

        public override string CmdUsage => "";

        public override string CmdDesc =>
            "Write the position and orientation of all entities in the vision in the file 'entities.txt'";

        public override bool Run(IClient handler, string command, out string responseMsg, Dictionary<string, object> localVars)
        {
            responseMsg = "";
            if (handler is not RyzomClient ryzomClient)
                throw new Exception("Command handler is not a Ryzom client.");

            var args = GetArgs(command);

            // Check parameters
            if (args.Length != 0)
            {
                responseMsg = $"Usage: {CmdUsage}";
                return false;
            }

            // Log entities
            ryzomClient.GetNetworkManager().GetEntityManager().WriteEntities();

            // Command well done.
            return true;
        }
    }
}