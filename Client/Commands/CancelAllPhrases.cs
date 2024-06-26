﻿using System;
using System.Collections.Generic;
using API;
using API.Commands;
using Client.Stream;

namespace Client.Commands
{
    public class CancelAllPhrases : CommandBase
    {
        public override string CmdName => "CancelAllPhrases";

        public override string CmdUsage => "";

        public override string CmdDesc => "Called to cancel a Phrase link.";

        public override string Run(IClient handler, string command, Dictionary<string, object> localVars)
        {
            if (!(handler is RyzomClient ryzomClient))
                throw new Exception("Command handler is not a Ryzom client.");

            // Check parameters.
            if (HasArg(command))
                return "Please specify no parameters.";

            // Create the message and send.
            const string msgName = "PHRASE:CANCEL_ALL";
            var out2 = new BitMemoryStream();

            if (ryzomClient.GetNetworkManager().GetMessageHeaderManager().PushNameToStream(msgName, out2))
                ryzomClient.GetNetworkManager().Push(out2);
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