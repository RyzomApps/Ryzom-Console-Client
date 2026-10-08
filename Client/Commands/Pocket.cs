///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
using API;
using API.Commands;
using Client.BotChat;
using System;
using System.Collections.Generic;

namespace Client.Commands
{
    /// <summary>
    /// Show the pocket slots (hotbar), with the referenced bag slot for each.
    /// </summary>
    public class Pocket : CommandBase
    {
        private const uint MaxPockets = 5;

        public override string CmdName => "Pocket";

        public override string CmdUsage => "";

        public override string CmdDesc => "Show the pocket slots (hotbar slots 1-5, with the referenced bag slot)";

        public override bool Run(IClient handler, string command, out string responseMsg, Dictionary<string, object> localVars)
        {
            responseMsg = "";
            if (handler is not RyzomClient ryzomClient)
                throw new Exception("Command handler is not a Ryzom client.");

            var lines = new List<string> { $"Pocket slots ({MaxPockets}):" };

            for (uint slot = 0; slot < MaxPockets; slot++)
            {
                var pNl = ryzomClient.GetDatabaseManager().GetServerNode($"INVENTORY:HOTBAR:{slot}:INDEX_IN_BAG", false);
                var bagIndex = pNl != null ? pNl.GetValue32() : 0;

                if (bagIndex <= 0)
                {
                    lines.Add($"  Pocket #{slot + 1}: empty");
                    continue;
                }

                var item = ryzomClient.GetInventoryManager().GetBagItem((uint)bagIndex);

                if (item == null || item.GetSheetId() == 0)
                {
                    lines.Add($"  Pocket #{slot + 1}: bag slot {bagIndex} (item not received yet)");
                    continue;
                }

                ryzomClient.GetStringManager().GetString(item.GetNameId(), out var name, ryzomClient.GetNetworkManager());
                if (string.IsNullOrEmpty(name))
                    name = $"sheet {item.GetSheetId()}";

                lines.Add($"  Pocket #{slot + 1}: bag slot {bagIndex,-4} qty {item.GetQuantity(),-6} q {item.GetQuality(),-3} {name}");
            }

            responseMsg = string.Join("\n", lines);
            return true;
        }
    }
}
