using API;
using API.Commands;
using System;
using System.Collections.Generic;

namespace Client.Commands
{
    public class AskPolitely : CommandBase
    {
        public override string CmdName => "AskPolitely";

        public override string CmdUsage => "[1=bullying]";

        public override string CmdDesc => "Show target url. E.g. \"Ask politely\".";

        public override bool Run(IClient handler, string command, out string responseMsg, Dictionary<string, object> localVars)
        {
            responseMsg = "";
            if (handler is not RyzomClient ryzomClient)
                throw new Exception("Command handler is not a Ryzom client.");

            var args = GetArgs(command);

            // Check parameters / Perform admin command
            if (args.Length > 0 && args[0] == "1")
                ryzomClient.PerformInternalCommand("a openTargetUrl 1", out responseMsg);
            else
                ryzomClient.PerformInternalCommand("a openTargetUrl", out responseMsg);

            return true;
        }
    }
}