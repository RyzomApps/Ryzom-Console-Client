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
    /// </summary>
    public class BotMissionContinue : CommandBase
    {

        public override CommandCategory CmdCategory => CommandCategory.Npc;
        public override string CmdName => "BotMissionContinue";

        public override string CmdUsage => "<index>";

        public override string CmdDesc => "Clicks the given 'continue mission' context-menu option of the targeted bot (mission turn-in / 'talk about the mission'). Index is the mission option index from 'Db TARGET:CONTEXT_MENU:MISSIONS_OPTIONS'.";

        public override bool Run(IClient handler, string command, out string responseMsg, Dictionary<string, object> localVars)
        {
            responseMsg = "";
            if (handler is not RyzomClient ryzomClient)
            {
                responseMsg = "Command handler is not a Ryzom client.";
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
