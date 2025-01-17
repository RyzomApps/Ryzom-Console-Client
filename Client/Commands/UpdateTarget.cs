using System;
using System.Collections.Generic;
using API;
using API.Commands;

namespace Client.Commands
{
    public class UpdateTarget : CommandBase
    {
        public override string CmdName => "UpdateTarget";

        public override string CmdUsage => "";

        public override string CmdDesc => "Update current target";

        public override string Run(IClient handler, string command, Dictionary<string, object> localVars)
        {
            if (!(handler is RyzomClient ryzomClient))
                throw new Exception("Command handler is not a Ryzom client.");

            var args = GetArgs(command);

            // Check parameters
            if (args.Length != 0)
                return "Wrong argument count in the command.";

            // Perform admin command
            ryzomClient.PerformInternalCommand("a updateTarget", out var response);
            return response;
        }

        public override IEnumerable<string> GetCmdAliases()
        {
            return new[] { "" };
        }
    }
}
