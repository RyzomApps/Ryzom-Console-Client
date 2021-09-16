using RCC.Helper;

namespace RCC.Network
{
    internal static class ChatMngr
    {
        public static void processTellString(CBitMemStream bms, object o)
        {
            uint CompressedIndex = 0;
            uint SenderNameId = 0;
            string Content = "";

            // Serial. For tell message, there is no chat mode, coz we know we are in tell mode !
            bms.serial(ref CompressedIndex);
            bms.serial(ref SenderNameId);
            bms.serial(ref Content);

            ConsoleIO.WriteLineFormatted($"§f[{CompressedIndex}]{SenderNameId}§r tells you: §f{Content}");
        }

        //const int dyn_chat = 12;
        //const int shout = 1;

        enum TGroupType
        {
            say = 0,
            shout,
            team,
            guild,
            civilization,
            territory,
            universe,
            tell,
            player,
            arround,
            system,
            region,
            dyn_chat,
            nbChatMode
            // Following mode are client side only. Thus, after 'nbChatMode'
        };

        public static void processChatString(CBitMemStream bms, object o)
        {
            //// before displaying anything, must ensure dynamic channels are up to date
            //// NB: only processChatString() have to do this. Other methods cannot be in dyn_chat mode
            //updateDynamicChatChannels(chatDisplayer);
            //
            //// serial
            //CChatMsg chatMsg;
            //bms.serial(chatMsg);

            var f = bms;

            uint CompressedIndex = 0;
            uint SenderNameId = 0;
            byte ChatMode = 0;
            //	uint32			DynChatChanID;
            long DynChatChanID = 0;
            string Content = "";

            f.serial(ref CompressedIndex);
            f.serial(ref SenderNameId);
            f.serial(ref ChatMode);
            if (ChatMode == (byte)TGroupType.dyn_chat)
                f.serial(ref DynChatChanID);
            f.serial(ref Content);

            //CChatGroup::TGroupType type = static_cast<CChatGroup::TGroupType>(chatMsg.ChatMode);
            //ucstring senderStr;
            //
            //bool complete = true;
            //complete &= STRING_MANAGER::CStringManagerClient::instance()->getString(chatMsg.SenderNameId, senderStr);
            //
            //if (type == CChatGroup::dyn_chat)
            //{
            //    // retrieve the DBIndex from the dynamic chat id
            //    sint32 dbIndex = ChatMngr.getDynamicChannelDbIndexFromId(chatMsg.DynChatChanID);
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
            // -> // void CClientChatManager::buildChatSentence(TDataSetIndex /* compressedSenderIndex */, const ucstring &sender, const ucstring &msg, CChatGroup::TGroupType type, ucstring &result)

            string color = "§f";

            switch ((TGroupType)ChatMode)
            {
                case TGroupType.dyn_chat: color = "§b"; break;
                case TGroupType.shout: color = "§c"; break;
                case TGroupType.team: color = "§9"; break;
                case TGroupType.guild: color = "§a"; break;
                case TGroupType.civilization: color = "§d"; break;
                case TGroupType.territory: color = "§d"; break;
                case TGroupType.universe: color = "§6"; break;
                case TGroupType.region: color = "§7"; break;
                case TGroupType.tell: color = "§f"; break;
                default: /*nlwarning("unknown group type"); return;*/ break;
            }

            switch ((TGroupType)ChatMode)
            {
                case TGroupType.shout:
                    ConsoleIO.WriteLineFormatted($"§f[{ChatMode}][{CompressedIndex}]{SenderNameId}§r shouts: {color}{Content}");
                    break;

                default:
                    ConsoleIO.WriteLineFormatted($"§f[{ChatMode}][{CompressedIndex}]{SenderNameId}§r says: {color}{Content}");
                    break;
            }
        }
    }
}