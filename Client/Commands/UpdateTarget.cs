using API;
using API.Commands;
using System;
using System.Collections.Generic;

namespace Client.Commands
{
    public class UpdateTarget : CommandBase
    {
        public override string CmdName => "UpdateTarget";

        public override string CmdUsage => "";

        public override string CmdDesc => "Update current target";

        public override bool Run(IClient handler, string command, out string responseMsg, Dictionary<string, object> localVars)
        {
            responseMsg = "";
            if (handler is not RyzomClient ryzomClient)
                throw new Exception("Command handler is not a Ryzom client.");

            var args = GetArgs(command);

            // Check parameters and perform admin command
            if (args.Length == 0)
                return ryzomClient.PerformInternalCommand("a updateTarget", out responseMsg);

            responseMsg = "Wrong argument count in the command.";
            return false;
        }
    }
}