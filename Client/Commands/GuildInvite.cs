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

        public override bool Run(IClient handler, string command, out string responseMsg, Dictionary<string, object> localVars)
        {
            responseMsg = "";
            if (handler is not RyzomClient ryzomClient)
                throw new Exception("Command handler is not a Ryzom client.");

            var args = GetArgs(command);

            // Check parameters
            if (args.Length != 1)
            {
                responseMsg = "Wrong argument count in the command.";
                return false;
            }

            // Perform admin command
            ryzomClient.PerformInternalCommand($"a guildInvite {args[0]}", out var response);
            {
                responseMsg = response;
                return false;
            }
        }
    }
}