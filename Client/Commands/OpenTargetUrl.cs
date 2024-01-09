using System;
using System.Collections.Generic;
using API;
using API.Commands;

namespace Client.Commands
{
    public class OpenTargetUrl : CommandBase
    {
        public override string CmdName => "OpenTargetUrl";

        public override string CmdUsage => "[1=bullying]";

        public override string CmdDesc => "Open target url. E.g. \"Ask politely\".";

        public override string Run(IClient handler, string command, Dictionary<string, object> localVars)
        {
            if (!(handler is RyzomClient ryzomClient))
                throw new Exception("Command handler is not a Ryzom client.");

            var args = GetArgs(command);

            // Check parameters / Perform admin command
            var response = "";

            if (args.Length > 0 && args[0] == "1")
                ryzomClient.PerformInternalCommand("a openTargetUrl 1", ref response);
            else
                ryzomClient.PerformInternalCommand("a openTargetUrl", ref response);

            return response;
        }

        public override IEnumerable<string> GetCmdAliases()
        {
            return new[] { "" };
        }
    }
}