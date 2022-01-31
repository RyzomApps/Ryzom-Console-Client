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
    /// Ask the server to rename a character
    /// </summary>
    public static class ActionHandlerRenameChar
    {
        /// <summary>
        /// Execute the answer to the action
        /// </summary>
        public static void Execute(byte nSelectedSlot, NetworkManager networkManager)
        {
            var sName = "Fuainunu";
            var sSurname = "NotSet";

            if (nSelectedSlot >= networkManager.CharacterSummaries.Count)
                return;

            // Create the message for the server to create the character.
            var out2 = new BitMemoryStream(false, 2);

            if (!networkManager.GetMessageHeaderManager().PushNameToStream("CONNECTION:RENAME_CHAR", out2))
            {
                RyzomClient.GetInstance().GetLogger().Warn("don't know message name CONNECTION:RENAME_CHAR");
                return;
            }

            // Get the selected slot
            out2.Serial(ref nSelectedSlot);
            out2.Serial(ref sName, false);
            out2.Serial(ref sSurname, false);

            networkManager.Push(out2);
            //networkManager.Send(networkManager.GetCurrentServerTick());

            RyzomClient.GetInstance().GetLogger().Info("impulseCallBack : CONNECTION:RENAME_CHAR sent...");

            //networkManager.WaitServerAnswer = true;
        }
    }
}