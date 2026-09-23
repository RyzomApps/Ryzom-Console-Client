using API;
using API.Commands;
using Client.Stream;
using System;
using System.Collections.Generic;

namespace Client.Commands
{
    public class GuildKick : CommandBase
    {
        public override string CmdName => "GuildKick";

        public override string CmdUsage => "<playerName> <counter>";

        public override string CmdDesc =>
            "Client (lead, ho, of) wants to kick member specifying its index. Last parameter is the counter.";

        public override bool Run(IClient handler, string command, out string responseMsg, Dictionary<string, object> localVars)
        {
            responseMsg = "";
            if (handler is not RyzomClient ryzomClient)
                throw new Exception("Command handler is not a Ryzom client.");

            var args = GetArgs(command);

            if (args.Length != 1)
            {
                responseMsg = "Please specify a parameter.";
                return false;
            }

            const string msgName = "GUILD:KICK_MEMBER";

            // TODO GUKick right arguments
            var out2 = new BitMemoryStream();

            if (!ryzomClient.GetNetworkManager().GetMessageHeaderManager().PushNameToStream(msgName, out2))
            {
                responseMsg = $"Unknown message named '{msgName}'.";
                return false;
            }

            var buf = args[0];
            out2.Serial(ref buf);
            ryzomClient.GetNetworkManager().Push(out2);

            return true;
        }
    }
}