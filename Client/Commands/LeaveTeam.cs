using System.Collections.Generic;
using Client.Commands.Internal;
using Client.Network;

namespace Client.Commands
{
    public class LeaveTeam : CommandBase
    {
        public override string CmdName => "leaveTeam";

        public override string CmdUsage => "";

        public override string CmdDesc => "leave team";

        public override string Run(RyzomClient handler, string command, Dictionary<string, object> localVars)
        {
            // Create the message for the server to execute a phrase.

            const string msgName = "TEAM:LEAVE";
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
            return new string[] { };
        }
    }
}