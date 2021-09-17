// This code is a modified version of a file from the 'Ryzom - MMORPG Framework'
// <http://dev.ryzom.com/projects/ryzom/>,
// which is released under GNU Affero General Public License.
// <http://www.gnu.org/licenses/>
// Original Copyright 2010 by Winch Gate Property Limited

using RCC.Helper;
using RCC.Network;

namespace RCC.Client
{
    /// <summary>
    /// Class for management of incoming and outgoing chat messages
    /// </summary>
    internal static class ChatManager
    {
        /// <summary>
        /// interprets the incoming tell string
        /// </summary>
        public static void ProcessTellString(BitMemoryStream bms, object o)
        {
            uint compressedIndex = 0;
            uint senderNameId = 0;
            string content = "";

            // Serial. For tell message, there is no chat mode, coz we know we are in tell mode !
            bms.Serial(ref compressedIndex);
            bms.Serial(ref senderNameId);
            bms.Serial(ref content);

            ConsoleIO.WriteLineFormatted($"§f[{compressedIndex}]{senderNameId}§r tells you: §f{content}");
        }

        /// <summary>
        /// interprets the incoming say string
        /// </summary>
        public static void ProcessChatString(BitMemoryStream bms, object o)
        {
            //// before displaying anything, must ensure dynamic channels are up to date
            //// NB: only processChatString() have to do this. Other methods cannot be in dyn_chat mode
            //updateDynamicChatChannels(chatDisplayer);
            //
            //// serial
            //CChatMsg chatMsg;
            //bms.serial(chatMsg);

            var f = bms;

            uint compressedIndex = 0;
            uint senderNameId = 0;
            byte chatMode = 0;
            //	uint32			DynChatChanID;
            long dynChatChanID = 0;
            string content = "";

            f.Serial(ref compressedIndex);
            f.Serial(ref senderNameId);
            f.Serial(ref chatMode);
            if (chatMode == (byte)ChatGroupType.DynChat)
                f.Serial(ref dynChatChanID);
            f.Serial(ref content);

            //CChatGroup::ChatGroupType type = static_cast<CChatGroup::ChatGroupType>(chatMsg.ChatMode);
            //ucstring senderStr;
            //
            //bool complete = true;
            //complete &= STRING_MANAGER::CStringManagerClient::instance()->getString(chatMsg.SenderNameId, senderStr);
            //
            //if (type == CChatGroup::dyn_chat)
            //{
            //    // retrieve the DBIndex from the dynamic chat id
            //    sint32 dbIndex = ChatManager.getDynamicChannelDbIndexFromId(chatMsg.DynChatChanID);
            //    // if the client database is not yet up to date, put the chat message in buffer
            //    if (dbIndex < 0)
            //        complete = false;
            //}
            //
            //// if !complete, wait
            //if (!complete)
            //{
            //    _ChatBuffer.push_back(CChatMsgNode(chatMsg, false));
            //    //nldebug("<impulseChat> Received CHAT, put in buffer : waiting association");
            //    return;
            //}
            //
            //// display
            //ucstring ucstr;
            //buildChatSentence(chatMsg.CompressedIndex, senderStr, chatMsg.Content, type, ucstr);
            //chatDisplayer.displayChat(chatMsg.CompressedIndex, ucstr, chatMsg.Content, type, chatMsg.DynChatChanID, senderStr);
            // -> // void CClientChatManager::buildChatSentence(TDataSetIndex /* compressedSenderIndex */, const ucstring &sender, const ucstring &msg, CChatGroup::ChatGroupType type, ucstring &result)

            var color = "§f";

            switch ((ChatGroupType)chatMode)
            {
                case ChatGroupType.DynChat:
                    color = "§b";
                    break;
                case ChatGroupType.Shout:
                    color = "§c";
                    break;
                case ChatGroupType.Team:
                    color = "§9";
                    break;
                case ChatGroupType.Guild:
                    color = "§a";
                    break;
                case ChatGroupType.Civilization:
                    color = "§d";
                    break;
                case ChatGroupType.Territory:
                    color = "§d";
                    break;
                case ChatGroupType.Universe:
                    color = "§6";
                    break;
                case ChatGroupType.Region:
                    color = "§7";
                    break;
                case ChatGroupType.Tell:
                    color = "§f";
                    break;
                default: /*nlwarning("unknown group type"); return;*/ break;
            }

            switch ((ChatGroupType)chatMode)
            {
                case ChatGroupType.Shout:
                    ConsoleIO.WriteLineFormatted(
                        $"§f[{chatMode}][{compressedIndex}]{senderNameId}§r shouts: {color}{content}");
                    break;

                default:
                    ConsoleIO.WriteLineFormatted(
                        $"§f[{chatMode}][{compressedIndex}]{senderNameId}§r says: {color}{content}");
                    break;
            }
        }
    }
}