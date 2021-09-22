///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

using RCC.Network;

namespace RCC.Chat
{
    public class ChatMsg
    {
        public ChatGroupType ChatMode;
        public uint CompressedIndex;
        public string Content;
        public uint DynChatChanID;
        public uint SenderNameId;

        public ChatMsg()
        {
            CompressedIndex = ChatManager.InvalidDatasetIndex;
            SenderNameId = 0;
            ChatMode = 0;
            DynChatChanID = 0;
            Content = "";
        }

        public void Serial(BitMemoryStream f)
        {
            f.Serial(ref CompressedIndex);
            f.Serial(ref SenderNameId);
            byte chatModeByte = 0;
            f.Serial(ref chatModeByte);
            ChatMode = (ChatGroupType) chatModeByte;
            if (ChatMode == ChatGroupType.DynChat)
                f.Serial(ref DynChatChanID);
            f.Serial(ref Content);
        }
    };
}