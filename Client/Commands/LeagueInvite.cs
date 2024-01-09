using System;
using System.Collections.Generic;
using API;
using API.Commands;

namespace Client.Commands
{
    public class LeagueInvite : CommandBase
    {
        public override string CmdName => "LeagueInvite";

        public override string CmdUsage => "<playername>";

        public override string CmdDesc => "Invites a player's team into your league. Invitation will go to the player's team's leader.";

        public override string Run(IClient handler, string command, Dictionary<string, object> localVars)
        {
            if (!(handler is RyzomClient ryzomClient))
                throw new Exception("Command handler is not a Ryzom client.");

            var args = GetArgs(command);

            // Check parameters
            if (args.Length != 1)
                return "Wrong argument count in the command.";

            // leagueinvite - Perform admin command
            var response = "";
            ryzomClient.PerformInternalCommand("a leagueInvite " + args[0], ref response);
            return response;
        }

        public override IEnumerable<string> GetCmdAliases()
        {
            return new[] { "" };
        }
    }
}