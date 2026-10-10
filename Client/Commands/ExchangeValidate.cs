///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////
using API;
using API.Commands;
using Client.Stream;
using System;
using System.Collections.Generic;
using System.Text;

namespace Client.Commands
{
    /// <summary>
    /// Validate the player trade proposal (send EXCHANGE:VALIDATE with the exchange counter).
    /// </summary>
    public class ExchangeValidate : CommandBase
    {

        public override CommandCategory CmdCategory => CommandCategory.Trade;
        public override string CmdName => "ExchangeValidate";

        public override string CmdUsage => "";

        public override string CmdDesc => "Validate (confirm) the player trade proposal. Use after 'ExchangePutItem'/'ExchangeMoney'";

        public override bool Run(IClient handler, string command, out string responseMsg, Dictionary<string, object> localVars)
        {
            responseMsg = "";
            if (handler is not RyzomClient ryzomClient)
                throw new Exception("Command handler is not a Ryzom client.");

            // The counter of the proposal (SERVER:EXCHANGE:COUNTER); the server
            // rejects the validation when it does not match.
            var counterNode = ryzomClient.GetDatabaseManager().GetNode("SERVER:EXCHANGE:COUNTER", false);
            if (counterNode == null)
            {
                responseMsg = "No exchange proposal active (SERVER:EXCHANGE:COUNTER missing). Use 'ExchangeProposal' first.";
                return false;
            }

            var counter = (byte)counterNode.GetValue32();

            const string msgName = "EXCHANGE:VALIDATE";
            var out2 = new BitMemoryStream();
            if (!ryzomClient.GetNetworkManager().GetMessageHeaderManager().PushNameToStream(msgName, out2))
            {
                responseMsg = $"Unknown message name '{msgName}'.";
                return false;
            }

            out2.Serial(ref counter);
            ryzomClient.GetNetworkManager().Push(out2);

            responseMsg = $"Validation sent (counter: {counter}). The other side must validate too; use 'ExchangeShow' to check.";
            return true;
        }
    }
}
