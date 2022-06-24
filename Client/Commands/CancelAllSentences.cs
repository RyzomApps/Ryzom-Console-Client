using System;
using System.Collections.Generic;
using API;
using API.Commands;
using Client.Network;
using Client.Stream;

namespace Client.Commands
{
    /// <summary>
    /// Cancel all the sentences being executed
    /// </summary>
    public class CancelAllSentences : CommandBase
    {
        public override string CmdName => "cancelAllSentences";

        public override string CmdUsage => "";

        public override string CmdDesc => "Cancel all the sentences being executed";

        public override string Run(IClient handler, string command, Dictionary<string, object> localVars)
        {
            // no parameter needed
            if (!(handler is RyzomClient ryzomClient))
                throw new Exception("Command handler is not a Ryzom client.");

            // Create the message for the server to cancel the phrase being executed
            const string msgName = "SENTENCE:CANCEL_ALL";

            var @out = new BitMemoryStream();

            if (ryzomClient.GetNetworkManager().GetMessageHeaderManager().PushNameToStream(msgName, @out))
                ryzomClient.GetNetworkManager().Push(@out);
            else
                return $"Unknown message named '{msgName}'.";

            return "";
        }

        public override IEnumerable<string> GetCmdAliases()
        {
            return new string[] { };
        }
    }
}