// This code is a modified version of a file from the 'Ryzom - MMORPG Framework'
// <http://dev.ryzom.com/projects/ryzom/>,
// which is released under GNU Affero General Public License.
// <http://www.gnu.org/licenses/>
// Original Copyright 2010 by Winch Gate Property Limited

namespace RCC.Client
{
    public enum ChatGroupType : byte
    {
        Say = 0,
        Shout,
        Team,
        Guild,
        Civilization,
        Territory,
        Universe,
        Tell,
        Player,
        Around,
        System,
        Region,
        DynChat,

        NbChatMode
        // Following mode are client side only. Thus, after 'nbChatMode'
    };
}