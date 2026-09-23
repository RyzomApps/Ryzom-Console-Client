using API;
using API.Commands;
using System;
using System.Collections.Generic;

namespace Client.Commands
{
    public class LeagueInvite : CommandBase
    {
        public override string CmdName => "LeagueInvite";

        public override string CmdUsage => "<playername>";

        public override string CmdDesc => "Invites a player's team into your league. Invitation will go to the player's team's leader.";

        public override bool Run(IClient handler, string command, out string responseMsg, Dictionary<string, object> localVars)
        {
            responseMsg = "";
            if (handler is not RyzomClient ryzomClient)
                throw new Exception("Command handler is not a Ryzom client.");

            var args = GetArgs(command);

            // Check parameters and perform admin command
            if (args.Length == 1)
                return ryzomClient.PerformInternalCommand($"a leagueInvite {args[0]}", out responseMsg);

            responseMsg = "Wrong argument count in the command.";
            return false;
        }
    }
}