///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////
using API;
using API.Commands;
using System.Collections.Generic;
using System.Linq;

namespace Client.Commands
{
    /// <summary>
    /// Display the target bot's mission context menu options
    /// (SERVER:TARGET:CONTEXT_MENU:MISSIONS_OPTIONS) as readable text.
    /// These are the indices to use with 'BotMissionContinue'.
    /// </summary>
    public class ContextMenu : CommandBase
    {
        public override CommandCategory CmdCategory => CommandCategory.Npc;

        public override string CmdName => "contextmenu";

        public override string CmdUsage => "";

        public override string CmdDesc => "Display the mission options of the targeted bot's context menu. Run 'target <bot>' first; the index shown for each option is the one to use with 'botmissioncontinue'.";

        /// <summary>
        /// Null-safe read from the SERVER bank (unprefixed paths, like 'missions').
        /// Falls back to the prefixed root path for older layouts.
        /// </summary>
        private static long ReadProp(global::Client.Database.DatabaseManager dbManager, string serverPath)
        {
            var node = dbManager.GetServerNode(serverPath, false);
            var v = node?.GetValue64() ?? -1;
            if (v == -1)
                v = dbManager.GetProp($"SERVER:{serverPath}");
            return v;
        }

        public override bool Run(IClient handler, string command, out string responseMsg, Dictionary<string, object> localVars)
        {
            responseMsg = "";
            if (handler is not RyzomClient ryzomClient)
            {
                responseMsg = "Command handler is not a Ryzom client.";
                return false;
            }

            var dbManager = ryzomClient.GetDatabaseManager();
            if (dbManager == null)
            {
                responseMsg = "Database manager not available.";
                return false;
            }

            // collect all set option title text ids (0 = no option)
            var ids = new uint[8];
            var gifts = new bool[8];
            var pending = new List<uint>();
            for (var i = 0; i < ids.Length; i++)
            {
                ids[i] = (uint)ReadProp(dbManager, $"TARGET:CONTEXT_MENU:MISSIONS_OPTIONS:{i}:TITLE");
                gifts[i] = ReadProp(dbManager, $"TARGET:CONTEXT_MENU:MISSIONS_OPTIONS:{i}:PLAYER_GIFT_NEEDED") != 0;
                if (ids[i] != 0 && !pending.Contains(ids[i]))
                    pending.Add(ids[i]);
            }

            // decode bot chat programmes (flags) offered by the targeted bot
            var programmes = (uint)ReadProp(dbManager, "TARGET:CONTEXT_MENU:PROGRAMMES");
            responseMsg = "Bot programmes: " + DecodeBotChatFlags(programmes) + $" (raw {programmes})\n";

            if (pending.Count == 0)
            {
                responseMsg = "No mission options in the context menu of the targeted bot.\n"
                    + "If all slots are 0 but programmes contain 'ChooseMission': the bot has no mission\n"
                    + "for you right now, or the current step's target bot is a DIFFERENT npc - check the\n"
                    + "mission goal ('missions') and target that bot.";
                return false;
            }

            // resolve all titles at once (pumps the network for pushes)
            var resolved = StringWait.WaitAll(ryzomClient.GetStringManager(), pending, ryzomClient.GetNetworkManager());

            responseMsg = "Context menu (target bot):";
            for (var i = 0; i < ids.Length; i++)
            {
                if (ids[i] == 0)
                    continue;
                var text = resolved.TryGetValue(ids[i], out var t) ? t : $"id {ids[i]}";
                responseMsg += $"\n option {i}: {text}";
                if (gifts[i])
                    responseMsg += " [GIFT NEEDED: bot expects items, use 'botgiftvalidate' after clicking this option]";
            }
            responseMsg += "\nClick an option with: botmissioncontinue <index>.";

            return true;
        }

        private static string DecodeBotChatFlags(uint flags)
        {
            string[] names =
            {
                "News", "TradeItem", "TradePact", "TradeBuilding", "GuildRoleMaster",
                "TradePhrase", "CreateGuild", "ChooseMission", "Attackable", "CancelGuildDuty",
                "AskGuildDuty", "TradeFaction", "TradeCosmetic", "TradeTeleport", "GuildInvite",
                "DontFollow", "WebPage", "Totem", "Outpost", "TradeOutpostBuilding"
            };
            var set = new List<string>();
            for (var b = 0; b < names.Length && b < 32; b++)
                if ((flags & (1u << b)) != 0)
                    set.Add(names[b]);
            return set.Count > 0 ? string.Join(", ", set) : "none";
        }
    }
}
