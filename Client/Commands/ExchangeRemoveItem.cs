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
    /// Remove an item from the player trade window.
    /// Sends EXCHANGE:REMOVE (index_in_exchange, quantity).
    /// </summary>
    public class ExchangeRemoveItem : CommandBase
    {

        public override CommandCategory CmdCategory => CommandCategory.Trade;
        public override string CmdName => "ExchangeRemoveItem";

        public override string CmdUsage => "<exchangeSlot>";

        public override string CmdDesc => "Remove an item from the player trade. Slot indices come from 'ExchangeShow'";

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

            var exchangeSlot = (ushort)Convert.ToInt32(args[0], 10);
            if (exchangeSlot >= 10)
            {
                responseMsg = "Exchange slot must be between 0 and 9.";
                return false;
            }

            const string msgName = "EXCHANGE:REMOVE";
            var out2 = new BitMemoryStream();
            if (!ryzomClient.GetNetworkManager().GetMessageHeaderManager().PushNameToStream(msgName, out2))
            {
                responseMsg = $"Unknown message name '{msgName}'.";
                return false;
            }

            out2.Serial(ref exchangeSlot);
            ryzomClient.GetNetworkManager().Push(out2);

            responseMsg = $"Remove requested for exchange slot {exchangeSlot}. Use 'ExchangeShow' to check.";
            return true;
        }
    }
}
