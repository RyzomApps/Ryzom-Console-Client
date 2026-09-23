using API;
using API.Commands;
using Client.Stream;
using System;
using System.Collections.Generic;

namespace Client.Commands
{
    public class JoinTeamProposal : CommandBase
    {
        public override string CmdName => "invite";

        public override string CmdUsage => "[name]";

        public override string CmdDesc => "Propose to the current target or the given player name to join the team.";

        public override bool Run(IClient handler, string command, out string responseMsg, Dictionary<string, object> localVars)
        {
            responseMsg = "";
            if (handler is not RyzomClient ryzomClient)
                throw new Exception("Command handler is not a Ryzom client.");

            var args = GetArgs(command);

            switch (args.Length)
            {
                // Check parameters.
                case > 1:
                    responseMsg = "Wrong argument count in the command.";
                    return false;

                case 0:
                    // Invite the target - Create the message for the server
                    const string msgName = "TEAM:JOIN_PROPOSAL";
                    var out2 = new BitMemoryStream();

                    if (ryzomClient.GetNetworkManager().GetMessageHeaderManager().PushNameToStream(msgName, out2))
                    {
                        ryzomClient.GetNetworkManager().Push(out2);
                    }
                    else
                    {
                        responseMsg = $"Unknown message named '{msgName}'.";
                        return true;
                    }

                    responseMsg = "";
                    return false;

                default:
                    // Invite a named player - Perform admin command
                    return ryzomClient.PerformInternalCommand($"a teamInvite {args[0]}", out responseMsg);
            }
        }

        public override IEnumerable<string> GetCmdAliases()
        {
            return ["joinTeamProposal"];
        }
    }
}