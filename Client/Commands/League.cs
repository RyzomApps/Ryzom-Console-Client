using System;
using System.Collections.Generic;
using API;
using API.Commands;

namespace Client.Commands
{
    public class League : CommandBase
    {
        public override string CmdName => "League";

        public override string CmdUsage => "<leaguename>";

        public override string CmdDesc => "Creates a league with the given name and brings your team into the league. Can only be used by team leader.";

        public override string Run(IClient handler, string command, Dictionary<string, object> localVars)
        {
            if (!(handler is RyzomClient ryzomClient))
                throw new Exception("Command handler is not a Ryzom client.");

            var args = GetArgs(command);

            // Check parameters
            if (args.Length != 1)
                return "Wrong argument count in the command.";

            // Perform admin command
            var response = "";
            ryzomClient.PerformInternalCommand($"a setLeague \"{args[0]}\"", ref response);
            return response;
        }

        public override IEnumerable<string> GetCmdAliases()
        {
            return new[] { "" };
        }
    }
}