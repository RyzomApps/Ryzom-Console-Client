using System;
using System.Collections.Generic;
using API;
using API.Commands;

namespace Client.Commands
{
    public class DssTarget : CommandBase
    {
        public override string CmdName => "DssTarget";

        public override string CmdUsage => "<arg>";

        public override string CmdDesc => "Ask DSS to perform a GM action on the player's target";

        public override bool Run(IClient handler, string command, out string responseMsg, Dictionary<string, object> localVars)
        {
            responseMsg = "";
            if (handler is not RyzomClient ryzomClient)
                throw new Exception("Command handler is not a Ryzom client.");

            var args = GetArgs(command);

            // Check parameters and perform admin command
            if (args.Length == 1)
                return ryzomClient.PerformInternalCommand($"a dssTarget {string.Join(' ', args)}", out responseMsg);

            responseMsg = "Wrong argument count in the command.";
            return false;
        }
    }
}