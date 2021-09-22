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
    public class ChatMsg2
    {
        public ChatGroupType ChatMode;
        public uint CompressedIndex;
        public string CustomTxt;
        public uint PhraseId;
        public uint SenderNameId;

        public ChatMsg2()
        {
            CompressedIndex = ChatManager.InvalidDatasetIndex;
            SenderNameId = 0;
            ChatMode = 0;
            PhraseId = 0;
            CustomTxt = "";
        }

        public void Serial(BitMemoryStream f)
        {
            f.Serial(ref CompressedIndex);
            f.Serial(ref SenderNameId);
            byte ChatModeByte = 0;
            f.Serial(ref ChatModeByte);
            ChatMode = (ChatGroupType) ChatModeByte;
            f.Serial(ref PhraseId);
            f.Serial(ref CustomTxt);
        }
    };
}