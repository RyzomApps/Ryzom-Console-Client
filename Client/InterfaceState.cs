///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

namespace RCC.Helper
{
    public enum InterfaceState
    {
        AutoLogin, // -> GLOBAL_MENU, QUIT (if connection errors)
        GlobalMenu, // -> SELECT_CHARACTER, QUIT (if connection errors)
        GoInTheGame, // -> launch the game
        QuitTheGame // -> quit the game
    };
}