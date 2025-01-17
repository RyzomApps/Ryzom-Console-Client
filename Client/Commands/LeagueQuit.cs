using System;
using System.Collections.Generic;
using API;
using API.Commands;

namespace Client.Commands
{
    public class LeagueQuit : CommandBase
    {
        public override string CmdName => "LeagueQuit";

        public override string CmdUsage => "";

        public override string CmdDesc => "Quits your league, quits your team from the league if you are team leader.";

        public override string Run(IClient handler, string command, Dictionary<string, object> localVars)
        {
            if (!(handler is RyzomClient ryzomClient))
                throw new Exception("Command handler is not a Ryzom client.");

            var args = GetArgs(command);

            // Check parameters
            if (args.Length != 0)
                return "Wrong argument count in the command.";

            // leaguequit - Perform admin command
            ryzomClient.PerformInternalCommand("a setLeague", out var response);
            return response;
        }

        public override IEnumerable<string> GetCmdAliases()
        {
            return new[] { "" };
        }
    }
}