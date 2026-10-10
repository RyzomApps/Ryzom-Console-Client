///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////
using API;
using API.BotChat;
using API.Commands;
using Client.BotChat;
using System;
using System.Collections.Generic;

namespace Client.Commands
{
    /// <summary>
    /// Set the filters of the trade list.
    /// </summary>
    public class BotTradeFilters : CommandBase
    {

        public override CommandCategory CmdCategory => CommandCategory.Npc;
        public override string CmdName => "BotTradeFilters";

        public override string CmdUsage => "<minQuality> <maxQuality> <minPrice> <maxPrice> [minClass maxClass itemPart itemType]";

        public override string CmdDesc => "Set the trade list filters. Use -1 for no filter (quality/price), 0 disables class/part/type filters";

        public override bool Run(IClient handler, string command, out string responseMsg, Dictionary<string, object> localVars)
        {
            responseMsg = "";
            if (handler is not RyzomClient ryzomClient)
                throw new Exception("Command handler is not a Ryzom client.");

            var args = GetArgs(command);
            if (args.Length is not (4 or 8))
            {
                responseMsg = $"Usage: {CmdUsage}";
                return false;
            }

            var botChatManager = ryzomClient.GetBotChatManager();
            if (botChatManager.CurrentSessionId == 0)
            {
                responseMsg = "No trade session running. Use 'BotTradeStart <type>' first.";
                return false;
            }

            const uint noFilter = uint.MaxValue;
            var minQuality = ParseUint(args[0], noFilter);
            var maxQuality = ParseUint(args[1], noFilter);
            var minPrice = ParseUint(args[2], noFilter);
            var maxPrice = ParseUint(args[3], noFilter);

            byte minClass = 0, maxClass = 0, itemPart = 0, itemType = 0;
            if (args.Length == 8)
            {
                minClass = (byte)ParseUint(args[4], 0);
                maxClass = (byte)ParseUint(args[5], 0);
                itemPart = (byte)ParseUint(args[6], 0);
                itemType = (byte)ParseUint(args[7], 0);
            }

            var sent = botChatManager.SendSetFilters(minQuality, maxQuality, minPrice, maxPrice, minClass, maxClass, itemPart, itemType);
            responseMsg = sent ? "Filters sent. Use 'BotTradeList' to display the filtered list." : "Failed to send the filters.";
            return sent;
        }

        private static uint ParseUint(string arg, uint defaultValue)
        {
            var value = Convert.ToInt32(arg, 10);
            return value < 0 ? defaultValue : (uint)value;
        }
    }
}
