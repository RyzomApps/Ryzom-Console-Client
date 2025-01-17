using System;
using System.Collections.Generic;
using API;
using API.Commands;

namespace Client.Commands
{
    public class AskPolitely : CommandBase
    {
        public override string CmdName => "AskPolitely";

        public override string CmdUsage => "[1=bullying]";

        public override string CmdDesc => "Show target url. E.g. \"Ask politely\".";

        public override string Run(IClient handler, string command, Dictionary<string, object> localVars)
        {
            if (!(handler is RyzomClient ryzomClient))
                throw new Exception("Command handler is not a Ryzom client.");

            var args = GetArgs(command);

            // Check parameters / Perform admin command
            string response;

            if (args.Length > 0 && args[0] == "1")
                ryzomClient.PerformInternalCommand("a openTargetUrl 1", out response);
            else
                ryzomClient.PerformInternalCommand("a openTargetUrl", out response);

            return response;
        }

        public override IEnumerable<string> GetCmdAliases()
        {
            return new[] { "" };
        }
    }
}