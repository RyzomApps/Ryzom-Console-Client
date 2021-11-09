///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

namespace Client.Chat
{
    /// <summary>
    /// Type of the chat channel (e.g. around, region, dynamic).
    /// </summary>
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
    };
}