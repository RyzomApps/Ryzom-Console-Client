﻿///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

using API.Chat;

namespace Client.Chat
{
    public interface IChatDisplayer
    {
        /// <param name="rawMessage">raw message</param>
        /// <param name="mode">in which channel should this message goes</param>
        /// <param name="dynChatId">is valid only if mode==dyn_chat. This the Id of channel (not the index in DB!)</param>
        /// <param name="compressedSenderIndex">id of sender name</param>
        /// <param name="ucstr">processed message</param>
        /// <param name="senderName">name of the sender of the message</param>
        /// <param name="bubbleTimer">timespan the bubble should be displayed</param>
        public void DisplayChat(uint compressedSenderIndex, string ucstr, string rawMessage, ChatGroupType mode, uint dynChatId, string senderName, uint bubbleTimer = 0);

        /// <summary>
        /// display a player tell message
        /// </summary>
        public void DisplayTell(string formattedMessage, string senderName, string rawMessage);
    }
}