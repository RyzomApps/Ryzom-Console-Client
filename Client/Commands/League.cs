using API;
using API.Commands;
using System;
using System.Collections.Generic;

namespace Client.Commands
{
    public class League : CommandBase
    {
        public override string CmdName => "League";

        public override string CmdUsage => "<leaguename>";

        public override string CmdDesc => "Creates a league with the given name and brings your team into the league. Can only be used by team leader.";

        public override bool Run(IClient handler, string command, out string responseMsg, Dictionary<string, object> localVars)
        {
            responseMsg = "";
            if (handler is not RyzomClient ryzomClient)
                throw new Exception("Command handler is not a Ryzom client.");

            var args = GetArgs(command);

            // Check parameters and perform admin command
            if (args.Length == 1)
                return ryzomClient.PerformInternalCommand($"a setLeague \"{args[0]}\"", out responseMsg);

            responseMsg = "Wrong argument count in the command.";
            return false;
        }
    }
}