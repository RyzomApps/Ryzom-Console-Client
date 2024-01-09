using System;
using System.Collections.Generic;
using API;
using API.Commands;

namespace Client.Commands
{
    public class ShowOnline : CommandBase
    {
        public override string CmdName => "ShowOnline";

        public override string CmdUsage => "<0|1|2>";

        public override string CmdDesc => "Set friend visibility mode:\n2\tGuild members\n1\tFriends and guild members\n0\tEveryone\nRequires relog.";

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
            ryzomClient.PerformInternalCommand("a showOnline " + args[0], ref response);
            return response;
        }

        public override IEnumerable<string> GetCmdAliases()
        {
            return new[] { "" };
        }
    }
}