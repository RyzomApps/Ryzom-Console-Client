///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

namespace Client.Client
{
    public enum InterfaceState
    {
        /// <summary>
        /// GLOBAL_MENU, QUIT (if connection errors)
        /// </summary>
        AutoLogin,
        /// <summary>
        /// SELECT_CHARACTER, QUIT (if connection errors)
        /// </summary>
        GlobalMenu,
        /// <summary>
        /// launch the game
        /// </summary>
        GoInTheGame,
        /// <summary>
        /// quit the game
        /// </summary>
        QuitTheGame
    };
}