using System;
using System.Collections.Generic;
using API;
using API.Commands;

namespace Client.Commands
{
    public class SetPvPTag : CommandBase
    {
        public override string CmdName => "SetPvPTag";

        public override string CmdUsage => "<tag>";

        public override string CmdDesc => "Set player character PvP tag to true or false";

        public override string Run(IClient handler, string command, Dictionary<string, object> localVars)
        {
            if (!(handler is RyzomClient ryzomClient))
                throw new Exception("Command handler is not a Ryzom client.");

            var args = GetArgs(command);

            // Check parameters
            if (args.Length != 1)
                return "Wrong argument count in the command.";

            // Perform admin command
            ryzomClient.PerformInternalCommand($"a setPvPTag {args[0]}", out var response);
            return response;
        }

        public override IEnumerable<string> GetCmdAliases()
        {
            return new[] { "" };
        }
    }
}