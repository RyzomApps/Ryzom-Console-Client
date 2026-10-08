using API;
using API.Commands;
using Client.Stream;
using System;
using System.Collections.Generic;

namespace Client.Commands
{
    /// <summary>
    /// GCM Exchange
    /// </summary>
    public class ExchangeProposal : CommandBase
    {
        public override string CmdName => "ExchangeProposal";

        public override string CmdUsage => "";

        public override string CmdDesc => "Propose an item exchange with the player in front. Then use 'ExchangePutItem'/'ExchangeMoney' and 'ExchangeValidate'";

        public override bool Run(IClient handler, string command, out string responseMsg, Dictionary<string, object> localVars)
        {
            responseMsg = "";
            if (handler is not RyzomClient ryzomClient)
                throw new Exception("Command handler is not a Ryzom client.");

            // Game Specific Code
            const string msgName = "EXCHANGE:PROPOSAL";
            var out2 = new BitMemoryStream();

            if (ryzomClient.GetNetworkManager().GetMessageHeaderManager().PushNameToStream(msgName, out2))
            {
                ryzomClient.GetNetworkManager().Push(out2);

                responseMsg = "Exchange proposal sent. Use 'ExchangePutItem'/'ExchangeMoney' to fill it and 'ExchangeShow' to check.";
                return true;
            }
            else
            {
                responseMsg = $"Unknown message named '{msgName}'.";
                return false;
            }
        }
    }
}