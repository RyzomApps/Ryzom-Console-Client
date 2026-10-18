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

namespace Client.Commands
{
    /// <summary>
    /// Tell the server that the targeted bot's mission (or the special bot
    /// context) can be advanced: the client sent the "continue mission"
    /// context-menu option. Index is the flattened bot chat option index
    /// as shown in the NPC context menu (SERVER:TARGET:CONTEXT_MENU:MISSIONS_OPTIONS).
    /// The server flattens all options of the targeted bot into one list:
    /// mission step options (talk/give steps of all missions, in server
    /// mission order - NOT journal slot order), then mission teleports,
    /// then the bot's own context texts, then auto missions. So the index
    /// from 'contextmenu' refers to that flattened list, not to a journal
    /// slot - the option can belong to any mission the bot is involved in.
    /// The server silently drops the message if no valid bot is targeted
    /// or the bot is farther than 8 m (MaxTalkingDist). A completed step
    /// is reported as "You have finished a mission step." and a changed
    /// goal text in 'missions'; the mission may stay in the journal with
    /// its next step. Dynamic missions open the bot's dynamic chat on the
    /// final talk step - finish it with 'dynchat' / 'dynchatclickoption'.
    /// </summary>
    public class BotMissionContinue : CommandBase
    {

        public override CommandCategory CmdCategory => CommandCategory.Npc;
        public override string CmdName => "BotMissionContinue";

        public override string CmdUsage => "<index>";

        public override string CmdDesc => "Clicks a 'continue mission' context-menu option of the targeted bot (bot must be targeted and within 8 m). Index is the 0-based option number shown by 'contextmenu' / 'Db TARGET:CONTEXT_MENU:MISSIONS_OPTIONS' - a flattened list over ALL missions of that bot, NOT the journal mission slot. A finished step shows as 'You have finished a mission step.' plus a changed goal text in 'missions'. Dynamic missions open the bot's dynamic chat on the final talk step - finish it with 'dynchat' / 'dynchatclickoption'.";

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


            var args = GetArgs(command);

            if (args.Length != 1 || !byte.TryParse(args[0], out var nOpt))
            {
                responseMsg = "Wrong argument count or argument could not be parsed.";
                return false;
            }

            // Create the message for the server
            const string sMsg = "BOTCHAT:CONTINUE_MISSION";
            var outStream = new BitMemoryStream();
            if (ryzomClient.GetNetworkManager().GetMessageHeaderManager().PushNameToStream(sMsg, outStream))
            {
                outStream.Serial(ref nOpt);
                ryzomClient.GetNetworkManager().Push(outStream);

                responseMsg = "Continue mission option sent.";
                return true;
            }

            responseMsg = $"Warning: unknown message name '{sMsg}'.";
            return false;
        }
    }
}
