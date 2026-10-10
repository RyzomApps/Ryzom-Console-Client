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
    /// Display the current player trade window (SERVER:EXCHANGE), 10 slots per page.
    /// </summary>
    public class ExchangeShow : CommandBase
    {

        public override CommandCategory CmdCategory => CommandCategory.Trade;
        private const uint DefaultPerPage = 10;

        public override string CmdName => "ExchangeShow";

        public override string CmdUsage => "[page]";

        public override string CmdDesc => "Display the current player trade window, 10 slots per page";

        public override bool Run(IClient handler, string command, out string responseMsg, Dictionary<string, object> localVars)
        {
            responseMsg = "";
            if (handler is not RyzomClient ryzomClient)
                throw new Exception("Command handler is not a Ryzom client.");

            var args = GetArgs(command);
            var page = args.Length == 1 ? (uint)Convert.ToUInt32(args[0], 10) : 0u;

            var builder = new StringBuilder();
            AppendExchangeSide(ryzomClient, builder, "GIVE", "You give", page);
            AppendExchangeSide(ryzomClient, builder, "RECEIVE", "You receive", page);

            var money = ExchangeDb.SafeProp(ryzomClient, "SERVER:EXCHANGE:MONEY", -1);
            if (money >= 0)
                builder.AppendLine($"Money proposed: {MoneyFormat.Format(money)} dappers (raw: {money})");
            else
                builder.AppendLine("No exchange proposal active.");

            responseMsg = builder.ToString().TrimEnd();
            return true;
        }

        private static void AppendExchangeSide(RyzomClient ryzomClient, StringBuilder builder, string branch, string label, uint page)
        {
            builder.AppendLine($"{label} (page {page + 1}):");

            var shown = 0;
            for (var i = page * DefaultPerPage; i < (page + 1) * DefaultPerPage; i++)
            {
                var sheet = ExchangeDb.SafeProp(ryzomClient, $"SERVER:EXCHANGE:{branch}:{i}:SHEET", -1);
                if (sheet < 0)
                    break; // branch not in the database (minimal config) - stop here
                if (sheet == 0)
                    continue;

                var quantity = ExchangeDb.SafeProp(ryzomClient, $"SERVER:EXCHANGE:{branch}:{i}:QUANTITY", 0);
                var quality = ExchangeDb.SafeProp(ryzomClient, $"SERVER:EXCHANGE:{branch}:{i}:QUALITY", 0);

                var name = $"sheet {sheet}";

                builder.AppendLine($"  [{i}] {name} q{quality} x{quantity}");
                shown++;
            }

            if (shown == 0)
                builder.AppendLine("  (empty)");
        }
    }
}
