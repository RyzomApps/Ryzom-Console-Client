///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////
using API;
using API.Commands;
using Client.BotChat;
using System;
using System.Collections.Generic;

namespace Client.Commands
{
    /// <summary>
    /// Accept a mission of the current mission list.
    /// </summary>
    public class BotMissionAccept : CommandBase
    {

        public override CommandCategory CmdCategory => CommandCategory.Npc;
        public override string CmdName => "BotMissionAccept";

        public override string CmdUsage => "<slot>";

        public override string CmdDesc => "Accept the mission in slot <slot> (0..7) of the current mission list";

        public override bool Run(IClient handler, string command, out string responseMsg, Dictionary<string, object> localVars)
        {
            responseMsg = "";
            if (handler is not RyzomClient ryzomClient)
                throw new Exception("Command handler is not a Ryzom client.");

            var args = GetArgs(command);
            if (args.Length != 1 || !byte.TryParse(args[0], out var slot))
            {
                responseMsg = $"Usage: {CmdUsage}";
                return false;
            }

            var botChatManager = ryzomClient.GetBotChatManager();

            if (botChatManager.CurrentMissionSessionId == 0)
            {
                responseMsg = "No mission session running. Use 'BotMissionOpen' first.";
                return false;
            }

            var sent = botChatManager.SendPickMission(slot);
            if (!sent)
            {
                responseMsg = $"Failed to pick the mission in slot {slot}.";
                return false;
            }

            responseMsg = $"Mission pick sent (slot: {slot}).";
            return true;
        }
    }
}
