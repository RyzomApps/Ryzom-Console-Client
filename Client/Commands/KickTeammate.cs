using System;
using System.Collections.Generic;
using API;
using API.Commands;
using Client.Network;

namespace Client.Commands
{
    public class KickTeammate : CommandBase
    {
        public override string CmdName => "kickTeammate";

        public override string CmdUsage => "";

        public override string CmdDesc => "kick someone from your team";

        public override string Run(IClient handler, string command, Dictionary<string, object> localVars)
        {
            if (!(handler is RyzomClient ryzomClient))
                throw new Exception("Command handler is not a Ryzom client.");

            // Create the message for the server to execute a phrase.
            const string msgName = "TEAM:KICK";
            var out2 = new BitMemoryStream();

            if (ryzomClient.GetNetworkManager().GetMessageHeaderManager().PushNameToStream(msgName, out2))
            {
                ryzomClient.GetNetworkManager().Push(out2);
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