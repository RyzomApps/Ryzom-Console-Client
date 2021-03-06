using System;
using System.Collections.Generic;
using API;
using API.Commands;
using Client.Network;
using Client.Stream;

namespace Client.Commands
{
    public class JoinTeamProposal : CommandBase
    {
        public override string CmdName => "invite";

        public override string CmdUsage => "";

        public override string CmdDesc => "Propose to the current target to join the team";

        public override string Run(IClient handler, string command, Dictionary<string, object> localVars)
        {
            if (!(handler is RyzomClient ryzomClient))
                throw new Exception("Command handler is not a Ryzom client.");

            // Create the message for the server to execute a phrase.
            const string msgName = "TEAM:JOIN_PROPOSAL";
            var out2 = new BitMemoryStream();

            if (ryzomClient.GetNetworkManager().GetMessageHeaderManager().PushNameToStream(msgName, out2))
            {
                ryzomClient.GetNetworkManager().Push(out2);
            }
            else
            {
                return $"Unknown message named '{msgName}'.";
            }

            return "";
        }

        public override IEnumerable<string> GetCmdAliases()
        {
            return new[] { "joinTeamProposal" };
        }
    }
}