using System;
using System.Collections.Generic;
using API;
using API.Commands;

namespace Client.Commands
{
    public class UserChannel : CommandBase
    {
        public override string CmdName => "UserChannel";

        public override string CmdUsage => "<channel>";

        public override string CmdDesc => "Connect to User Channel Chat";

        public override string Run(IClient handler, string command, Dictionary<string, object> localVars)
        {
            if (!(handler is RyzomClient ryzomClient))
                throw new Exception("Command handler is not a Ryzom client.");

            var args = GetArgs(command);

            // Check parameters
            if (args.Length < 1)
                return "Wrong argument count in the command.";

            // Perform admin command
            ryzomClient.PerformInternalCommand($"a connectUserChannel {string.Join(' ', args)}", out var response);
            return response;
        }

        public override IEnumerable<string> GetCmdAliases()
        {
            return new[] { "" };
        }
    }
}