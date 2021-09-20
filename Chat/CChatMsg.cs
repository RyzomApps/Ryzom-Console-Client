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
    public class CChatMsg
    {
        public uint CompressedIndex;
        public uint SenderNameId;
        public ChatGroupType ChatMode;
        public uint DynChatChanID;
        public string Content;

        public CChatMsg()
        {
            CompressedIndex = ChatManager.INVALID_DATASET_INDEX;
            SenderNameId = 0;
            ChatMode = 0;
            DynChatChanID = 0;
            Content = "";
        }

        public void Serial(BitMemoryStream f)
        {
            f.Serial(ref CompressedIndex);
            f.Serial(ref SenderNameId);
            byte ChatModeByte = 0;
            f.Serial(ref ChatModeByte);
            ChatMode = (ChatGroupType)ChatModeByte;
            if (ChatMode == ChatGroupType.DynChat)
                f.Serial(ref DynChatChanID);
            f.Serial(ref Content);
        }
    };

}
