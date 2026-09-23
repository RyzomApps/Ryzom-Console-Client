using API;
using API.Commands;
using System;
using System.Collections.Generic;

namespace Client.Commands
{
    public class ShowOnline : CommandBase
    {
        public override string CmdName => "ShowOnline";

        public override string CmdUsage => "<0|1|2>";

        public override string CmdDesc => "Set friend visibility mode:\n2\tGuild members\n1\tFriends and guild members\n0\tEveryone\nRequires relog.";

        public override bool Run(IClient handler, string command, out string responseMsg, Dictionary<string, object> localVars)
        {
            responseMsg = "";
            if (handler is not RyzomClient ryzomClient)
                throw new Exception("Command handler is not a Ryzom client.");

            var args = GetArgs(command);

            // Check parameters and perform admin command
            if (args.Length == 1)
                return ryzomClient.PerformInternalCommand($"a showOnline {args[0]}", out responseMsg);

            responseMsg = "Wrong argument count in the command.";
            return false;
        }
    }
}