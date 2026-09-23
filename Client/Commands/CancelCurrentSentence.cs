using API;
using API.Commands;
using Client.Stream;
using System;
using System.Collections.Generic;

namespace Client.Commands
{
    /// <summary>
    /// Cancel the sentence being executed
    /// </summary>
    public class CancelCurrentSentence : CommandBase
    {
        public override string CmdName => "cancelCurrentSentence";

        public override string CmdUsage => "";

        public override string CmdDesc => "Cancel the sentence being executed";

        public override bool Run(IClient handler, string command, out string responseMsg, Dictionary<string, object> localVars)
        {
            responseMsg = "";
            // no parameter needed
            if (handler is not RyzomClient ryzomClient)
                throw new Exception("Command handler is not a Ryzom client.");

            // Create the message for the server to cancel the phrase being executed
            const string msgName = "SENTENCE:CANCEL_CURRENT";

            var @out = new BitMemoryStream();

            if (ryzomClient.GetNetworkManager().GetMessageHeaderManager().PushNameToStream(msgName, @out))
            {
                ryzomClient.GetNetworkManager().Push(@out);
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