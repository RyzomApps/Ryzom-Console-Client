///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
using API;
using API.Commands;
using Client.Stream;
using System;
using System.Collections.Generic;
using System.Text;

namespace Client.Commands
{
    /// <summary>
    /// Put money (in raw seeds) into the player trade window.
    /// Sends EXCHANGE:MONEY (money).
    /// </summary>
    public class ExchangeMoney : CommandBase
    {
        public override string CmdName => "ExchangeMoney";

        public override string CmdUsage => "<amount>";

        public override string CmdDesc => "Put money (raw seed value) into the player trade. Use 'ExchangeProposal' first";

        public override bool Run(IClient handler, string command, out string responseMsg, Dictionary<string, object> localVars)
        {
            responseMsg = "";
            if (handler is not RyzomClient ryzomClient)
                throw new Exception("Command handler is not a Ryzom client.");

            var args = GetArgs(command);
            if (args.Length != 1)
            {
                responseMsg = $"Usage: {CmdUsage}";
                return false;
            }

            var quantity = (long)Convert.ToInt64(args[0], 10);
            if (quantity < 0)
            {
                responseMsg = "Amount must be positive.";
                return false;
            }

            const string msgName = "EXCHANGE:SEEDS";
            var out2 = new BitMemoryStream();
            if (!ryzomClient.GetNetworkManager().GetMessageHeaderManager().PushNameToStream(msgName, out2))
            {
                responseMsg = $"Unknown message name '{msgName}'.";
                return false;
            }

            out2.Serial(ref quantity);
            ryzomClient.GetNetworkManager().Push(out2);

            responseMsg = $"Money request sent: {quantity} raw seeds. Use 'ExchangeShow' to check.";
            return true;
        }
    }
}
