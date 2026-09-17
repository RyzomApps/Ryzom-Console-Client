using System;
using System.Collections.Generic;
using API;
using API.Commands;
using Client.Stream;

namespace Client.Commands
{
    public class Who : CommandBase
    {
        public override string CmdName => "who";
        public override string CmdUsage => "[GM|channelName]";
        public override string CmdDesc => "Display all players currently in region";

        public override bool Run(IClient handler, string command, out string responseMsg, Dictionary<string, object> localVars)
        {
            responseMsg = "";
            if (handler is not RyzomClient ryzomClient)
                throw new Exception("Command handler is not a Ryzom client.");

            // Check parameters.
            if (GetArgs(command).Length > 1)
            {
                responseMsg = "Please specify less parameters.";
                return false;
            }

            const string msgName = "DEBUG:WHO";
            var out2 = new BitMemoryStream();

            if (!ryzomClient.GetNetworkManager().GetMessageHeaderManager().PushNameToStream(msgName, out2))
            {
                responseMsg = $"Unknown message named '{msgName}'.";
                return false;
            }

            var opt = "";

            if (GetArgs(command).Length == 1) opt = GetArgs(command)[0];

            out2.Serial(ref opt, false);
            ryzomClient.GetNetworkManager().Push(out2);

            return true;
        }
    }
}