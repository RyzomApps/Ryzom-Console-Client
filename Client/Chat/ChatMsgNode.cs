///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

namespace RCC.Chat
{
    public class ChatMsgNode
    {
        public ChatGroupType ChatMode;

        public uint CompressedIndex;

        /// <summary>
        /// For Chat and Tell messages
        /// </summary>
        public string Content;

        /// <summary>
        /// displayTell() or displayChat()
        /// </summary>
        public bool DisplayAsTell;

        public uint DynChatChanID;

        /// <summary>
        /// For Chat2 and Tell2 messages
        /// </summary>
        public uint PhraseId;

        public uint SenderNameId;

        /// <summary>
        /// Use PhraseId or Content?
        /// </summary>
        public bool UsePhraseId;

        public ChatMsgNode(ChatMsg chatMsg, bool displayAsTell)
        {
            CompressedIndex = chatMsg.CompressedIndex;
            SenderNameId = chatMsg.SenderNameId;
            ChatMode = chatMsg.ChatMode;
            DynChatChanID = chatMsg.DynChatChanID;
            Content = chatMsg.Content;
            PhraseId = 0;
            UsePhraseId = false;
            DisplayAsTell = displayAsTell;
        }

        public ChatMsgNode(ChatMsg2 chatMsg, bool displayAsTell)
        {
            CompressedIndex = chatMsg.CompressedIndex;
            SenderNameId = chatMsg.SenderNameId;
            ChatMode = chatMsg.ChatMode;
            DynChatChanID = 0;
            PhraseId = chatMsg.PhraseId;
            UsePhraseId = true;
            DisplayAsTell = displayAsTell;
        }
    };
}