///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

namespace Client.Chat
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
            CompressedIndex = Constants.InvalidDatasetIndex;
            SenderNameId = 0;
            ChatMode = 0;
            PhraseId = 0;
            CustomTxt = "";
        }
    };
}