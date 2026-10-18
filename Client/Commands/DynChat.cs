///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////
using API;
using API.Commands;
using Client.Network;
using System.Collections.Generic;

namespace Client.Commands
{
    /// <summary>
    /// Display the currently open NPC dyn chat as readable text:
    /// the description and the clickable options. The text comes from
    /// the dyn string container filled by the last BOTCHAT:DYNCHAT_OPEN.
    /// </summary>
    public class DynChat : CommandBase
    {
        public override CommandCategory CmdCategory => CommandCategory.Npc;

        public override string CmdName => "dynchat";

        public override string CmdUsage => "";

        public override string CmdDesc => "Display the currently open NPC chat: description and clickable options. Run 'botmissionopen' (or similar) first; the index shown for each option is the one to use with 'dynchatclickoption'.";

        public override bool CmdAsync => true;
        public override bool Run(IClient handler, string command, out string responseMsg, Dictionary<string, object> localVars)
        {
            responseMsg = "";
            if (handler is not RyzomClient ryzomClient)
            {
                responseMsg = "Command handler is not a Ryzom client.";
                return false;
            }
            // The server silently drops BOTCHAT messages when the bot is
            // farther than MaxTalkingDist (8 m) - fail early with a hint.
            var proximityError = BotChatProximity.Check(ryzomClient);
            if (proximityError != null)
            {
                responseMsg = proximityError;
                return false;
            }


            var networkManager = ryzomClient.GetNetworkManager();

            var dynStrs = networkManager.LastDynChatIds;
            if (dynStrs == null || dynStrs.Count == 0)
            {
                responseMsg = "No dyn chat open. Target a bot and run 'botmissionopen' first.";
                return false;
            }

            // resolve all string ids at once (pumps the network for pushes)
            var resolved = StringWait.WaitAll(ryzomClient.GetStringManager(), dynStrs, networkManager);

            string botName = null;
            if (resolved.TryGetValue(networkManager.LastDynChatBotName, out var nameStr))
                botName = nameStr;

            responseMsg = "Dyn chat";
            if (!string.IsNullOrEmpty(botName))
                responseMsg += $" [{botName}]";
            responseMsg += ":";

            for (var i = 0; i < dynStrs.Count; i++)
            {
                var prefix = i == 0 ? " desc: " : $" option {i - 1}: ";
                var text = resolved.TryGetValue(dynStrs[i], out var t) ? t : $"id {dynStrs[i]}";
                responseMsg += $"\n{prefix}{text}";
            }

            return true;
        }
    }
}
