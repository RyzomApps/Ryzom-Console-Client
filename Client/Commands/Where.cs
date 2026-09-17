using System;
using System.Collections.Generic;
using API;
using API.Commands;
using Client.Stream;

namespace Client.Commands
{
    public class Where : CommandBase
    {
        public override string CmdName => "where";

        public override string CmdUsage => "";

        public override string CmdDesc => "Ask information on the position";

        public override bool Run(IClient handler, string command, out string responseMsg, Dictionary<string, object> localVars)
        {
            responseMsg = "";
            if (handler is not RyzomClient ryzomClient)
                throw new Exception("Command handler is not a Ryzom client.");

            // Check parameters.
            if (HasArg(command))
            {
                responseMsg = "Please specify no parameters.";
                return false;
            }

            // Create the message and send.
            const string msgName = "COMMAND:WHERE";
            var out2 = new BitMemoryStream();

            if (ryzomClient.GetNetworkManager().GetMessageHeaderManager().PushNameToStream(msgName, out2))
            {
                ryzomClient.GetNetworkManager().Push(out2);
            }
            else
            {
                responseMsg = $"Unknown message named '{msgName}'.";
                return false;
            }

            return true;
        }
    }
}