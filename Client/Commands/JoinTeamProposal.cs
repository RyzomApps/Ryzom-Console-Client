using System;
using System.Collections.Generic;
using API;
using API.Commands;
using Client.Stream;

namespace Client.Commands
{
    public class JoinTeamProposal : CommandBase
    {
        public override string CmdName => "invite";

        public override string CmdUsage => "[name]";

        public override string CmdDesc => "Propose to the current target or the given player name to join the team.";

        public override string Run(IClient handler, string command, Dictionary<string, object> localVars)
        {
            if (!(handler is RyzomClient ryzomClient))
                throw new Exception("Command handler is not a Ryzom client.");

            var args = GetArgs(command);

            // Check parameters.
            if (args.Length > 1)
                return "Wrong argument count in the command.";

            if (args.Length == 0)
            {
                // Invite the target - Create the message for the server
                const string msgName = "TEAM:JOIN_PROPOSAL";
                var out2 = new BitMemoryStream();

                if (ryzomClient.GetNetworkManager().GetMessageHeaderManager().PushNameToStream(msgName, out2))
                    ryzomClient.GetNetworkManager().Push(out2);
                else
                    return $"Unknown message named '{msgName}'.";

                return "";
            }

            // Invite a named player - Perform admin command
            var response = "";
            ryzomClient.PerformInternalCommand("a teamInvite " + args[0], ref response);
            return response;
        }

        public override IEnumerable<string> GetCmdAliases()
        {
            return new[] { "joinTeamProposal" };
        }
    }
}