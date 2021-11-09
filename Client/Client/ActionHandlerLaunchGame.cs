///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

using Client.Config;
using Client.Network;

namespace Client.Client
{
    /// <summary>
    /// Launch the game given a slot (slot is reference to the character summaries)
    /// </summary>
    public static class ActionHandlerLaunchGame
    {
        /// <summary>
        /// Gets the player selected slot and sends it to the server
        /// </summary>
        public static void Execute(string sSlot, NetworkManager networkManager)
        {
            // Get the player selected slot
            if (sSlot != "ingame_auto")
            {
                networkManager.PlayerSelectedSlot = (byte)int.Parse(sSlot); //result.getInteger());
                if (networkManager.PlayerSelectedSlot >= networkManager.CharacterSummaries.Count)
                    return;

                ClientConfig.SelectCharacter = networkManager.PlayerSelectedSlot;
            }

            // Select the right sheet to create the user character.
            ClientConfig.UserSheet = networkManager.CharacterSummaries[networkManager.PlayerSelectedSlot].SheetId.ToString();

            // Send CONNECTION:SELECT_CHAR
            var out2 = new BitMemoryStream(false, 2);
            networkManager.GetMessageHeaderManager().PushNameToStream("CONNECTION:SELECT_CHAR", out2);

            var c = networkManager.PlayerSelectedSlot;
            out2.Serial(ref c);
            networkManager.Push(out2);

            RyzomClient.GetInstance().GetLogger().Info("Selection of the character in slot " + networkManager.PlayerSelectedSlot + " sent...");

            networkManager.WaitServerAnswer = true;
        }
    }
}