using System.Collections.Generic;
using RCC.Commands.Internal;
using RCC.Network;

namespace RCC.Commands
{
    public class JoinTeamProposal : CommandBase
    {
        public override string CmdName => "joinTeamProposal";

        public override string CmdUsage => "";

        public override string CmdDesc => "propose to current target to join the team";

        public override string Run(RyzomClient handler, string command, Dictionary<string, object> localVars)
        {
            // Create the message for the server to execute a phrase.
            const string msgName = "TEAM:JOIN_PROPOSAL";
            var out2 = new BitMemoryStream();

            if (handler.GetNetworkManager().GetMessageHeaderManager().PushNameToStream(msgName, out2))
            {
                handler.GetNetworkManager().Push(out2);
            }
            else
            {
                handler.GetLogger().Warn("mainLoop : unknown message name : '%s'" + msgName);
            }

            return "";
        }

        public override IEnumerable<string> GetCmdAliases()
        {
            return new[] { "invite" };
        }
    }
}