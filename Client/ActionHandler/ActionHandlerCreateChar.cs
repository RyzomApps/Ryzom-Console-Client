///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

using API;
using Client.Client;
using Client.Messages;
using Client.Stream;

namespace Client.ActionHandler
{
    /// <summary>
    /// Ask the server to create a character
    /// </summary>
    public class ActionHandlerCreateChar : ActionHandlerBase
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public ActionHandlerCreateChar(IClient client) : base(client) { }

        /// <summary>
        /// Execute the request to create a character
        /// </summary>
        public override void Execute(object caller, string parameters)
        {
            var slot = byte.Parse(GetParam(parameters, "slot")); // u8
            var name = GetParam(parameters, "name"); // s

            var ryzomClient = (RyzomClient)_client;

            // Create the message for the server to create the character
            var @out = new BitMemoryStream();

            if (!ryzomClient.GetNetworkManager().GetMessageHeaderManager().PushNameToStream("CONNECTION:CREATE_CHAR", @out))
            {
                ryzomClient.GetLogger().Error("Don't know message name CONNECTION:CREATE_CHAR");
                return;
            }

            // Build the character summary
            var cs = new CharacterSummary
            {
                Name = name,
                Mainland = 101
            };

            // Create the message to send to the server from the character summary
            var createCharMsg = new CreateCharMsg();
            createCharMsg.SetupFromCharacterSummary(cs, ryzomClient.GetSheetIdFactory());
            createCharMsg.Slot = slot;

            // Setup the new career
            createCharMsg.NbPointFighter = 1;
            createCharMsg.NbPointCaster = 2;
            createCharMsg.NbPointCrafter = 1;
            createCharMsg.NbPointHarvester = 1;

            // Setup starting point
            createCharMsg.StartPoint = 20; // New NewbieLand start village (starting_city)

            // Send the message to the server
            createCharMsg.SerialBitMemStream(@out);
            ryzomClient.GetNetworkManager().Push(@out);
            ryzomClient.GetNetworkManager().Send(ryzomClient.GetNetworkConnection().GetCurrentServerTick());

            ryzomClient.GetNetworkManager().WaitServerAnswer = true;

            ryzomClient.GetLogger().Debug("impulseCallBack : CONNECTION:CREATE_CHAR sent");
        }
    }
}