using API;
using API.Commands;
using Client.Stream;
using System;
using System.Collections.Generic;

namespace Client.Commands
{
    public class ExchangeAccept : CommandBase
    {
        public override string CmdName => "ExchangeAccept";

        public override string CmdUsage => "";

        public override string CmdDesc => "Accept an exchange invitation of the other player (use 'ExchangeValidate' to confirm your own proposal)";

        public override bool Run(IClient handler, string command, out string responseMsg, Dictionary<string, object> localVars)
        {
            responseMsg = "";
            if (handler is not RyzomClient ryzomClient)
                throw new Exception("Command handler is not a Ryzom client.");

            const string msgName = "EXCHANGE:ACCEPT_INVITATION";
            var out2 = new BitMemoryStream();

            if (ryzomClient.GetNetworkManager().GetMessageHeaderManager().PushNameToStream(msgName, out2))
            {
                ryzomClient.GetNetworkManager().Push(out2);

                responseMsg = "Exchange invitation accepted. Fill your side with 'ExchangePutItem'/'ExchangeMoney', then use 'ExchangeValidate'.";
                return true;
            }
            else
            {
                responseMsg = $"Unknown message name '{msgName}'";
                return false;
            }
        }
    }
}