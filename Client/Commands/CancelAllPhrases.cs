using API;
using API.Commands;
using Client.Stream;
using System;
using System.Collections.Generic;

namespace Client.Commands
{
    public class CancelAllPhrases : CommandBase
    {
        public override string CmdName => "CancelAllPhrases";

        public override string CmdUsage => "";

        public override string CmdDesc => "Called to cancel a Phrase link.";

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
            const string msgName = "PHRASE:CANCEL_ALL";
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