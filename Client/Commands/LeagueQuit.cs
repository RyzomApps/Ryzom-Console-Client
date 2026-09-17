using System;
using System.Collections.Generic;
using API;
using API.Commands;

namespace Client.Commands
{
    public class LeagueQuit : CommandBase
    {
        public override string CmdName => "LeagueQuit";

        public override string CmdUsage => "";

        public override string CmdDesc => "Quits your league, quits your team from the league if you are team leader.";

        public override bool Run(IClient handler, string command, out string responseMsg, Dictionary<string, object> localVars)
        {
            responseMsg = "";
            if (handler is not RyzomClient ryzomClient)
                throw new Exception("Command handler is not a Ryzom client.");

            var args = GetArgs(command);

            // Check parameters and perform admin command
            if (args.Length == 0)
                return ryzomClient.PerformInternalCommand("a setLeague", out responseMsg);

            responseMsg = "Wrong argument count in the command.";
            return false;
        }
    }
}