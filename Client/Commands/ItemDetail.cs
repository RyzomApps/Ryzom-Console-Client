///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////
using API;
using API.Commands;
using Client.Inventory;
using Client.Config;
using Client.Database;
using Client.Network;
using API.Helper;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Client.Commands
{
    /// <summary>
    /// Shows detailed information about one inventory item: the base fields
    /// from the inventory database branch plus the enriched ITEM_INFO:SET
    /// data (enchantment, hp/damage, protections, requirements, sap load, ...).
    /// Missing or outdated item info is requested from the server with
    /// ITEM_INFO:GET, like getItemInfo in the original client.
    /// </summary>
    public class ItemDetail : CommandBase
    {
        public override CommandCategory CmdCategory => CommandCategory.Inventory;

        public override string CmdName => "ItemDetail";

        public override string CmdUsage => "[inventory] <slot>";

        public override string CmdDesc =>
            "Show the details of one inventory item. Inventory: bag (default), temp, pet0..pet6, "
            + "give, receive, trading, share, guild, room or the raw index 0-18. "
            + "Item info is requested from the server when outdated - the command waits a few "
            + "seconds for the answer and prints it.";

        public override bool CmdAsync => true;

        /// <summary>
        /// Max time the command blocks the main loop waiting for the
        /// ITEM_INFO:SET answer after sending ITEM_INFO:GET.
        /// </summary>
        private const int InfoTimeoutMs = 3000;
        public override bool Run(IClient handler, string command, out string responseMsg, Dictionary<string, object> localVars)
        {
            responseMsg = "";
            if (handler is not RyzomClient ryzomClient)
                throw new Exception("Command handler is not a Ryzom client.");

            var args = GetArgs(command);
            if (args.Length < 1)
            {
                responseMsg = $"Usage: {CmdUsage}";
                return false;
            }

            string inventoryArg = "bag";
            string slotArg;
            if (args.Length == 1)
            {
                slotArg = args[0];
            }
            else
            {
                inventoryArg = args[0];
                slotArg = args[1];
            }

            if (!ItemSlotId.TryResolveInventory(inventoryArg, out var inventoryIndex))
            {
                responseMsg = $"Unknown inventory '{inventoryArg}'. Use bag, temp, pet0..pet6, give, receive, trading, share, guild, room or 0-18.";
                return false;
            }
            if (!uint.TryParse(slotArg, out var slot))
            {
                responseMsg = $"Invalid slot '{slotArg}'.";
                return false;
            }

            var dbBranch = ryzomClient.GetDatabaseManager().GetServerBranch(ItemSlotId.DbPath(inventoryIndex));
            if (dbBranch == null)
            {
                responseMsg = $"Inventory branch {ItemSlotId.DbPath(inventoryIndex)} not found in the database.";
                return false;
            }

            // The db lists the slots as numbered children, like the
            // inventory managers of the original client do.
            var itemBranch = dbBranch.GetNode((ushort)slot) as DatabaseNodeBranch;
            if (itemBranch == null)
            {
                responseMsg = $"Slot {inventoryArg} {slot} is empty (no db node).";
                return true;
            }

            var item = new ItemImage();
            item.Build(itemBranch);
            if (item.GetSheetId() == 0)
            {
                responseMsg = $"Slot {inventoryArg} {slot} is empty.";
                return true;
            }

            var lines = new List<string>
            {
                $"Item: {ItemNameResolver.Resolve(handler, item.GetNameId(), item.GetSheetId())}"
            };
            if (item.GetWorned())
                lines.Add("  [BROKEN]");
            lines.Add($"  quality={item.GetQuality()} quantity={item.GetQuantity()} weight={item.GetWeight() / 10f:0.#} kg price={item.GetPrice()}");
            if (item.GetUserColor() != 0)
                lines.Add($"  color={item.GetUserColor()} characBuffs={item.GetCharacBuffs()}");
            if (item.GetResaleFlag() != 0)
                lines.Add($"  resaleFlag={item.GetResaleFlag()}{(item.GetLockedByOwner() ? " (locked by owner)" : "")}");

            // Enriched info: ITEM_INFO:SET data keyed by the wire slot id.
            var slotId = ItemSlotId.Encode(inventoryIndex, slot);
            var networkManager = ryzomClient.GetNetworkManager();
            var info = networkManager.GetItemInfos().GetValueOrDefault(slotId);
            if (info == null || info.VersionInfo != item.GetInfoVersion())
            {
                // Ask for fresh info like the original client (impulse
                // ITEM_INFO:GET); it arrives with the next ITEM_INFO:SET.
                networkManager.SendImpulse("ITEM_INFO:GET", out2 =>
                {
                    out2.U32(slotId);
                });

                // Pump the network until the answer lands, like StringWait
                // does: Update() dispatches the incoming ITEM_INFO:SET into
                // the item infos map, Send() flushes the outgoing queue so
                // the GET actually leaves while the main thread is blocked.
                var stopwatch = Stopwatch.StartNew();
                while (info == null || info.VersionInfo != item.GetInfoVersion())
                {
                    if (stopwatch.ElapsedMilliseconds >= InfoTimeoutMs)
                        break;
                    networkManager.Update();
                    networkManager.Send(networkManager.GetNetworkConnection().GetCurrentServerTick());
                    info = networkManager.GetItemInfos().GetValueOrDefault(slotId);
                    System.Threading.Thread.Sleep(10);
                }
            }

            if (info != null && info.VersionInfo == item.GetInfoVersion())
            {
                foreach (var detail in info.ToDetailLines())
                    lines.Add($"  {detail}");
            }
            else
            {
                lines.Add("  item info requested from the server - result follows...");
            }

            responseMsg = string.Join("\n", lines);
            return true;
        }
    }
}
