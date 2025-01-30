///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

using API;
using Client.Config;
using Client.Network;
using Client.Stream;

namespace Client.ActionHandler
{
    /// <summary>
    /// Launch the game given a slot (slot is reference to the character summaries)
    /// </summary>
    public class ActionHandlerLaunchGame : ActionHandlerBase
    {
        private readonly NetworkManager _networkManager;

        /// <summary>
        /// Constructor
        /// </summary>
        public ActionHandlerLaunchGame(IClient client, NetworkManager networkManager) : base(client)
        {
            _networkManager = networkManager;
        }

        /// <summary>
        /// Gets the player selected slot and sends it to the server
        /// </summary>
        public override void Execute(object caller, string parameters)
        {
            // Get the player selected slot from parameters
            var sSlot = GetParam(parameters, "slot");

            // Get the player selected slot
            if (sSlot != "ingame_auto")
            {
                _networkManager.PlayerSelectedSlot = (byte)int.Parse(sSlot);
                if (_networkManager.PlayerSelectedSlot >= _networkManager.CharacterSummaries.Count)
                    return;

                ClientConfig.SelectCharacter = _networkManager.PlayerSelectedSlot;
            }

            // Select the right sheet to create the user character.
            ClientConfig.UserSheet = _networkManager.CharacterSummaries[_networkManager.PlayerSelectedSlot].SheetId.ToString();

            // Send CONNECTION:SELECT_CHAR
            var out2 = new BitMemoryStream(false, 2);
            _networkManager.GetMessageHeaderManager().PushNameToStream("CONNECTION:SELECT_CHAR", out2);

            var c = _networkManager.PlayerSelectedSlot;
            out2.Serial(ref c);
            _networkManager.Push(out2);

            RyzomClient.GetInstance().GetLogger().Info($"Selection of the character in slot {_networkManager.PlayerSelectedSlot} sent...");

            _networkManager.WaitServerAnswer = true;
        }
    }
}