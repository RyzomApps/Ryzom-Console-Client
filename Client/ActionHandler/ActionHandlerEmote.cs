///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

using System;
using API;
using API.Entity;
using Client.Stream;

namespace Client.ActionHandler
{
    public class ActionHandlerEmote(IClient client) : ActionHandlerBase(client)
    {
        public override void Execute(object handler, string parameters)
        {
            if (_client is not RyzomClient ryzomClient)
                throw new Exception("Command handler is not a Ryzom client.");

            // An emote is 2 things: a phrase and an animation
            // Phrase is the phrase that server returns in chat system
            // Behav is the animation played
            // CustomPhrase is an user phrase which can replace default phrase
            string phraseNb = GetParam(parameters, "nb");
            string behav = GetParam(parameters, "behav");
            string customPhrase = GetParam(parameters, "custom_phrase");

            uint phraseNbValue = 0;
            if (!string.IsNullOrEmpty(phraseNb))
            {
                _ = uint.TryParse(phraseNb, out phraseNbValue);
            }

            byte behaviour = 255; // Default to 255 (no animation)
            if (!string.IsNullOrEmpty(behav))
            {
                _ = byte.TryParse(behav, out behaviour);
            }

            // Enum is sent as sint32
            int behavToSend = (byte)(behaviour + Constants.BehaviourEmoteBegin);
            ushort phraseNbToSend = (ushort)phraseNbValue;

            // Check if we should use idle animation
            if (behaviour == 255)
            {
                behavToSend = Constants.BehaviourIdle;
            }

            // TODO: Check for EAM (Emote Animation Manager) if available
            // TODO: Check if dead, stunned, or swimming

            if (string.IsNullOrEmpty(customPhrase))
            {
                // Send regular emote
                const string msgName = "COMMAND:EMOTE";
                var outStream = new BitMemoryStream();

                if (ryzomClient.GetNetworkManager().GetMessageHeaderManager().PushNameToStream(msgName, outStream))
                {
                    outStream.Serial(ref behavToSend);
                    outStream.Serial(ref phraseNbToSend);
                    ryzomClient.GetNetworkManager().Push(outStream);
                }
                else
                {
                    ryzomClient.GetLogger().Warn($"command 'emote': unknown message named '{msgName}'.");
                }
            }
            else
            {
                // Send custom emote
                const string msgName = "COMMAND:CUSTOM_EMOTE";
                var outStream = new BitMemoryStream();

                if (ryzomClient.GetNetworkManager().GetMessageHeaderManager().PushNameToStream(msgName, outStream))
                {
                    // Handle special case for "none"
                    if (customPhrase == "none")
                    {
                        if (behavToSend == Constants.BehaviourIdle)
                        {
                            // Display "no animation for emote" message
                            ryzomClient.GetLogger().Warn("No animation for emote");
                            return;
                        }
                    }
                    else
                    {
                        // Add &EMT& prefix and player name
                        var displayName = EntityHelper.RemoveTitleAndShardFromName(ryzomClient.GetNetworkManager().PlayerSelectedHomeShardName);
                        customPhrase = $"&EMT&{displayName} {customPhrase}";
                    }

                    outStream.Serial(ref behavToSend);
                    outStream.Serial(ref customPhrase); // ucstring (16 bits per character)
                    ryzomClient.GetNetworkManager().Push(outStream);
                }
                else
                {
                    ryzomClient.GetLogger().Warn($"Command 'emote': Unknown message named '{msgName}'.");
                }
            }
        }
    }
}