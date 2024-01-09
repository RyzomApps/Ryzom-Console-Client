using System;
using System.Collections.Generic;
using API;
using API.Commands;

namespace Client.Commands
{
    public class GuildInvite : CommandBase
    {
        public override string CmdName => "GuildInvite";

        public override string CmdUsage => "<playername>";

        public override string CmdDesc => "Invites a player to your guild. Can only be used by officers, high officers and the guild leader.";

        public override string Run(IClient handler, string command, Dictionary<string, object> localVars)
        {
            if (!(handler is RyzomClient ryzomClient))
                throw new Exception("Command handler is not a Ryzom client.");

            var args = GetArgs(command);

            // Check parameters
            if (args.Length != 1)
                return "Wrong argument count in the command.";

            // Perform admin command
            var response = "";
            ryzomClient.PerformInternalCommand("a guildInvite " + args[0], ref response);
            return response;
        }

        public override IEnumerable<string> GetCmdAliases()
        {
            return new[] { "" };
        }
    }
}