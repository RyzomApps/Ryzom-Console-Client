using API;
using API.Commands;
using Client.Stream;
using System;
using System.Collections.Generic;

namespace Client.Commands
{
    public class GuildQuit : CommandBase
    {
        public override string CmdName => "GuildQuit";

        public override string CmdUsage => "";

        public override string CmdDesc => "Client wants to quit its guild";

        public override bool Run(IClient handler, string command, out string responseMsg, Dictionary<string, object> localVars)
        {
            responseMsg = "";
            if (handler is not RyzomClient ryzomClient)
                throw new Exception("Command handler is not a Ryzom client.");

            var args = GetArgs(command);

            if (args.Length != 0)
            {
                responseMsg = "Please specify no parameters.";
                return false;
            }

            const string msgName = "GUILD:QUIT";
            var out2 = new BitMemoryStream();

            if (ryzomClient.GetNetworkManager().GetMessageHeaderManager().PushNameToStream(msgName, out2))
                ryzomClient.GetNetworkManager().Push(out2);

            return true;
        }
    }
}