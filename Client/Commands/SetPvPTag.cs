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

        public override bool Run(IClient handler, string command, out string responseMsg, Dictionary<string, object> localVars)
        {
            responseMsg = "";
            if (handler is not RyzomClient ryzomClient)
                throw new Exception("Command handler is not a Ryzom client.");

            var args = GetArgs(command);

            // Check parameters and perform admin command
            if (args.Length == 1)
                return ryzomClient.PerformInternalCommand($"a setPvPTag {args[0]}", out responseMsg);

            responseMsg = "Wrong argument count in the command.";
            return false;
        }
    }
}