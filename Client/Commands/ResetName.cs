using System;
using System.Collections.Generic;
using API;
using API.Commands;

namespace Client.Commands
{
    public class ResetName : CommandBase
    {
        public override string CmdName => "ResetName";

        public override string CmdUsage => "";

        public override string CmdDesc => "Reset player's name: Undo a temporary rename.";

        public override string Run(IClient handler, string command, Dictionary<string, object> localVars)
        {
            if (!(handler is RyzomClient ryzomClient))
                throw new Exception("Command handler is not a Ryzom client.");

            var args = GetArgs(command);

            // Check parameters
            if (args.Length != 0)
                return "Wrong argument count in the command.";

            // resetname - Perform admin command
            ryzomClient.PerformInternalCommand("a resetName", out var response);
            return response;
        }

        public override IEnumerable<string> GetCmdAliases()
        {
            return new[] { "" };
        }
    }
}