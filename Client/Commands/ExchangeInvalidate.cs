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
    /// Invalidate the current player trade proposal (clear own slots and money).
    /// Sends EXCHANGE:INVALIDATE.
    /// </summary>
    public class ExchangeInvalidate : CommandBase
    {

        public override CommandCategory CmdCategory => CommandCategory.Trade;
        public override string CmdName => "ExchangeInvalidate";

        public override string CmdUsage => "";

        public override string CmdDesc => "Invalidate the current player trade proposal (clears own slots and money)";

        public override bool Run(IClient handler, string command, out string responseMsg, Dictionary<string, object> localVars)
        {
            responseMsg = "";
            if (handler is not RyzomClient ryzomClient)
                throw new Exception("Command handler is not a Ryzom client.");

            const string msgName = "EXCHANGE:INVALIDATE";
            var out2 = new BitMemoryStream();

            if (ryzomClient.GetNetworkManager().GetMessageHeaderManager().PushNameToStream(msgName, out2))
            {
                ryzomClient.GetNetworkManager().Push(out2);
            }
            else
            {
                responseMsg = $"Unknown message name '{msgName}'.";
                return false;
            }

            responseMsg = "Invalidation sent. Use 'ExchangeShow' to check.";
            return true;
        }
    }
}
