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
    /// Validates the player gift of the current bot chat session
    /// (after putting the required items with the exchange commands).
    /// Server side this is cbClientValidateMissionGift -> acceptExchange(0).
    /// </summary>
    public class BotGiftValidate : CommandBase
    {

        public override CommandCategory CmdCategory => CommandCategory.Npc;
        public override string CmdName => "BotGiftValidate";

        public override string CmdUsage => "";

        public override string CmdDesc => "Validates the gift for a mission that requires items. Put the items into the gift slot with the exchange commands first, then call this.";

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


            GetArgs(command); // no arguments

            const string sMsg = "BOTCHAT:VALIDATE_PLAYER_GIFT";
            var outStream = new BitMemoryStream();
            if (ryzomClient.GetNetworkManager().GetMessageHeaderManager().PushNameToStream(sMsg, outStream))
            {
                ryzomClient.GetNetworkManager().Push(outStream);

                responseMsg = "Gift validation sent.";
                return true;
            }

            responseMsg = $"Warning: unknown message name '{sMsg}'.";
            return false;
        }
    }
}
