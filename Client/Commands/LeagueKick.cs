using System;
using System.Collections.Generic;
using API;
using API.Commands;

namespace Client.Commands
{
    public class LeagueKick : CommandBase
    {
        public override string CmdName => "LeagueKick";

        public override string CmdUsage => "<playername>";

        public override string CmdDesc => "Kick a person or team (if the person is team leader) out of your league.";

        public override string Run(IClient handler, string command, Dictionary<string, object> localVars)
        {
            if (!(handler is RyzomClient ryzomClient))
                throw new Exception("Command handler is not a Ryzom client.");

            var args = GetArgs(command);

            // Check parameters
            if (args.Length != 1)
                return "Wrong argument count in the command.";

            // Perform admin command
            ryzomClient.PerformInternalCommand($"a leagueKick {args[0]}", out var response);
            return response;
        }

        public override IEnumerable<string> GetCmdAliases()
        {
            return new[] { "" };
        }
    }
}