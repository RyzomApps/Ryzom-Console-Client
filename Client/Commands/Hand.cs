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
    /// Show the items currently held in the right and left hand,
    /// with the referenced bag slot for each of them.
    /// </summary>
    public class Hand : CommandBase
    {
        public override string CmdName => "Hand";

        public override string CmdUsage => "";

        public override string CmdDesc => "Show the items held in the right and left hand (with the referenced bag slot)";

        public override bool Run(IClient handler, string command, out string responseMsg, Dictionary<string, object> localVars)
        {
            responseMsg = "";
            if (handler is not RyzomClient ryzomClient)
                throw new Exception("Command handler is not a Ryzom client.");

            var entries = ryzomClient.GetInventoryManager().GetHandEntries();

            var lines = new List<string> { "Hand (right = slot 0, left = slot 1):" };

            for (var slot = 0; slot < 2; slot++)
            {
                var label = slot == 0 ? "right" : "left ";
                var found = false;

                var dbManager = ryzomClient.GetDatabaseManager();
                var pNl = dbManager.GetServerNode($"INVENTORY:HAND:{slot}:INDEX_IN_BAG", false);
                var bagIndex = pNl != null ? pNl.GetValue32() : 0;

                if (bagIndex <= 0)
                {
                    lines.Add($"  {label}: empty");
                    continue;
                }

                foreach (var entry in entries)
                {
                    if (entry.Index != (uint)bagIndex)
                        continue;

                    var item = entry.Item;
                    ryzomClient.GetStringManager().GetString(item.GetNameId(), out var name, ryzomClient.GetNetworkManager());
                    if (string.IsNullOrEmpty(name))
                        name = $"sheet {item.GetSheetId()}";

                    lines.Add($"  {label}: bag slot {bagIndex,-4} qty {item.GetQuantity(),-6} q {item.GetQuality(),-3} {name}");
                    found = true;
                    break;
                }

                if (!found)
                    lines.Add($"  {label}: bag slot {bagIndex} (item not received yet)");
            }

            responseMsg = string.Join("\n", lines);
            return true;
        }
    }
}
