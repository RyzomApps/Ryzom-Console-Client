///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

using API.Chat;
using Client.Stream;

namespace Client.Messages
{
    /// <summary>
    /// Message to chat
    /// </summary>
    /// <author>Boris Boucher</author> 
    /// <author>Nevrax France</author> 
    /// <date>2002</date> 
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

        internal void Serial(BitMemoryStream f)
        {
            f.Serial(ref CompressedIndex);
            f.Serial(ref SenderNameId);
            var chatMode = new byte();
            f.Serial(ref chatMode);
            ChatMode = (ChatGroupType)chatMode;
            f.Serial(ref PhraseId);
            f.Serial(ref CustomTxt, false); // FIXME: UTF-8 (serial)
        }
    }
}