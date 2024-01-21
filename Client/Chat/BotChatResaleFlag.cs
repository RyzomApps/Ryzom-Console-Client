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
    /// meaning of INVENTORY:BAG:i:RESALE_FLAG
    /// </summary>
    public enum BotChatResaleFlag : byte
    {
        /// <summary>this item can be resold</summary>
        ResaleOk = 0,
        /// <summary>this item can't be sold because it is partially broken</summary>
        ResaleKOBroken,
        /// <summary>this item can't be sold because its Resold time has expired</summary>
        ResaleKONoTimeLeft,
        /// <summary>this item can't be sold because the owner has locked it</summary>
        ResaleKOLockedByOwner,

        NumBotChatResaleFlag
    }
}
