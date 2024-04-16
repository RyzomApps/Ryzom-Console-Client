﻿///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

using API;
using API.Network;
using Client.Client;
using Client.Messages;
using Client.Network;
using Client.Sheet;
using Client.Stream;

namespace Client.ActionHandler
{
    /// <summary>
    /// Ask the server to create a character
    /// </summary>
    public static class ActionHandlerAskCreateChar
    {
        /// <summary>
        /// Execute the answer to the action
        /// </summary>
        public static void Execute(string name, byte slot, IClient client)
        {
            var ryzomClient = (RyzomClient)client;

            // Create the message for the server to create the character
            var @out = new BitMemoryStream();

            if (!ryzomClient.GetNetworkManager().GetMessageHeaderManager().PushNameToStream("CONNECTION:CREATE_CHAR", @out))
            {
                ryzomClient.GetLogger().Warn("Don't know message name CONNECTION:CREATE_CHAR");
                return;
            }

            // Build the character summary
            var cs = new CharacterSummary {
                //Mainland = ryzomClient.GetNetworkManager().MainlandSelected,
                Name = name,
            };

            // Create the message to send to the server from the character summary
            var createCharMsg = new CreateCharMsg();
            createCharMsg.SetupFromCharacterSummary(cs, ryzomClient.GetSheetIdFactory());
            createCharMsg.Slot = slot;

            // Setup the new career
            createCharMsg.NbPointFighter = 2;
            createCharMsg.NbPointCaster = 1; 
            createCharMsg.NbPointCrafter = 1;
            createCharMsg.NbPointHarvester = 1;

            // Setup starting point
            createCharMsg.StartPoint = 0; // kaemon

            // Send the message to the server
            createCharMsg.SerialBitMemStream(@out);
            ryzomClient.GetNetworkManager().Push(@out);
            ryzomClient.GetNetworkManager().Send();

            ryzomClient.GetNetworkManager().WaitServerAnswer = true;

            RyzomClient.GetInstance().GetLogger().Debug("impulseCallBack : CONNECTION:CREATE_CHAR sent");
        }
    }
}