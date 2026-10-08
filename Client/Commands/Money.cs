///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

using API;
using API.Commands;
using System;
using System.Collections.Generic;

namespace Client.Commands
{
    /// <summary>
    /// Display the money of the player (SERVER:INVENTORY:MONEY) and, when a
    /// player trade is open, the money currently proposed in the exchange.
    /// </summary>
    public class Money : CommandBase
    {
        public override string CmdName => "Money";

        public override string CmdUsage => "";

        public override string CmdDesc => "Display the current money of the player (dappers)";

        public override bool Run(IClient handler, string command, out string responseMsg, Dictionary<string, object> localVars)
        {
            responseMsg = "";
            if (handler is not RyzomClient ryzomClient)
                throw new Exception("Command handler is not a Ryzom client.");

            var node = ryzomClient.GetDatabaseManager().GetNode("SERVER:INVENTORY:MONEY", false);
            if (node == null)
            {
                responseMsg = "Money is not known yet (SERVER:INVENTORY:MONEY missing). Try again after login.";
                return false;
            }

            var money = node.GetValue64();
            responseMsg = $"Money: {MoneyFormat.Format(money)} dappers (raw: {money}).";

            // When a player trade window is open, show the proposed money too
            var exchangeNode = ryzomClient.GetDatabaseManager().GetNode("SERVER:EXCHANGE:MONEY", false);
            if (exchangeNode != null && exchangeNode.GetValue64() > 0)
                responseMsg += $"\nProposed in exchange: {MoneyFormat.Format(exchangeNode.GetValue64())} dappers (use 'ExchangeShow').";

            return true;
        }
    }
}
