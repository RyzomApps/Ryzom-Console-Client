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
    /// Use/consume the item stored in one of the pocket slots (hotbar 1-5).
    /// Sends ITEM:USE_ITEM with the referenced bag slot.
    /// </summary>
    public class UsePocket : CommandBase
    {

        public override CommandCategory CmdCategory => CommandCategory.Inventory;
        private const uint MaxPockets = 5;

        public override string CmdName => "UsePocket";

        public override string CmdUsage => "<slot 1-5>";

        public override string CmdDesc => "Use the item in the given pocket slot (hotbar slots 1-5, same mapping as the Pocket command)";

        public override bool Run(IClient handler, string command, out string responseMsg, Dictionary<string, object> localVars)
        {
            responseMsg = "";

            // Get the slot from the arguments
            var res = GetArgs(command);
            if (res.Length == 0)
            {
                responseMsg = $"Usage: {CmdUsage}";
                return false;
            }

            if (!uint.TryParse(res[0], out var slot) || slot < 1 || slot > MaxPockets)
            {
                responseMsg = $"Invalid pocket slot '{res[0]}'. Use 1-{MaxPockets}.";
                return false;
            }

            if (handler is not RyzomClient ryzomClient)
                throw new Exception("Command handler is not a Ryzom client.");

            slot--;

            var dbManager = ryzomClient.GetDatabaseManager();
            var pNl = dbManager.GetServerNode($"INVENTORY:HOTBAR:{slot}:INDEX_IN_BAG", false);
            var bagIndex = pNl != null ? pNl.GetValue32() : 0;

            if (bagIndex <= 0)
            {
                responseMsg = $"Pocket #{slot + 1} is empty.";
                return false;
            }

            var item = ryzomClient.GetInventoryManager().GetBagItem((uint)bagIndex);
            var name = item != null && item.GetSheetId() != 0
                ? API.Helper.ItemNameResolver.Resolve(handler, item.GetNameId(), item.GetSheetId())
                : $"bag slot {bagIndex}";

            if (!ryzomClient.GetInventoryManager().Use((uint)bagIndex, out var error))
            {
                responseMsg = $"Failed to use pocket #{slot + 1} ({name}): {error}";
                return false;
            }

            responseMsg = $"Using pocket #{slot + 1}: {name}";
            return true;
        }
    }
}
