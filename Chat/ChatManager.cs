///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Diagnostics;
using RCC.Client;
using RCC.Network;

namespace RCC.Chat
{
    /// <summary>
    ///     Class for management of incoming and outgoing chat messages
    /// </summary>
    internal static class ChatManager
    {
        public const uint INVALID_DATASET_INDEX = 0x00FFFFFF;

        const int PreTagSize = 5;
        static readonly List<ChatMsgNode> _ChatBuffer = new List<ChatMsgNode>();

        /// <summary>
        ///     interprets the incoming tell string
        /// </summary>
        public static void ProcessTellString(BitMemoryStream bms, IChatDisplayer chatDisplayer)
        {
            ChatMsg chatMsg = new ChatMsg();

            // Serial. For tell message, there is no chat mode, coz we know we are in tell mode !
            bms.Serial(ref chatMsg.CompressedIndex);
            bms.Serial(ref chatMsg.SenderNameId);
            bms.Serial(ref chatMsg.Content);

            chatMsg.ChatMode = ChatGroupType.Tell;

            // If !complete, wait
            bool complete = true;
            complete &= StringManagerClient.GetString(chatMsg.SenderNameId, out string senderStr);
            if (!complete)
            {
                _ChatBuffer.Add(new ChatMsgNode(chatMsg, true));
                RyzomClient.Log.Debug("<impulseTell> Received TELL, put in buffer : waiting association");
                return;
            }

            // display
            BuildTellSentence(senderStr, chatMsg.Content, out string ucstr);
            chatDisplayer.DisplayTell( /*chatMsg.CompressedIndex, */ucstr, senderStr);
        }

        /// <summary>
        ///     interprets the incoming say string
        /// </summary>
        public static void ProcessChatString(BitMemoryStream bms, IChatDisplayer chatDisplayer)
        {
            //// before displaying anything, must ensure dynamic channels are up to date
            //// NB: only processChatString() have to do this. Other methods cannot be in dyn_chat mode
            //updateDynamicChatChannels(chatDisplayer); TODO
            //
            //// serial
            ChatMsg chatMsg = new ChatMsg();
            chatMsg.Serial(bms);

            ChatGroupType type = chatMsg.ChatMode;
            string senderStr;

            bool complete = true;
            complete = StringManagerClient.GetString(chatMsg.SenderNameId, out senderStr);

            if (type == ChatGroupType.DynChat)
            {
                //// retrieve the DBIndex from the dynamic chat id
                //sint32 dbIndex = ChatManager.getDynamicChannelDbIndexFromId(chatMsg.DynChatChanID); TODO
                //// if the client database is not yet up to date, put the chat message in buffer
                //if (dbIndex < 0)
                //    complete = false;
            }

            // if !complete, wait
            if (!complete)
            {
                _ChatBuffer.Add(new ChatMsgNode(chatMsg, false));
                //nldebug("<impulseChat> Received CHAT, put in buffer : waiting association");
                return;
            }

            // display
            string ucstr;
            BuildChatSentence(chatMsg.CompressedIndex, senderStr, chatMsg.Content, type, out ucstr);
            chatDisplayer.DisplayChat(chatMsg.CompressedIndex, ucstr, chatMsg.Content, type, chatMsg.DynChatChanID,
                senderStr);
        }

        internal static void ProcessChatStringWithNoSender(BitMemoryStream bms, ChatGroupType type,
            IChatDisplayer chatDisplayer)
        {
            Debug.Assert(type != ChatGroupType.DynChat);

            // serial
            ChatMsg2 chatMsg = new ChatMsg2();
            uint phraseID = 0;
            bms.Serial(ref phraseID);
            //if (PermanentlyBanned) return;
            chatMsg.CompressedIndex = INVALID_DATASET_INDEX;
            chatMsg.SenderNameId = 0;
            chatMsg.ChatMode = type;
            chatMsg.PhraseId = phraseID;

            // if !complete, wait
            bool complete = StringManagerClient.GetDynString(chatMsg.PhraseId, out string ucstr);

            if (!complete)
            {
                _ChatBuffer.Add(new ChatMsgNode(chatMsg, false));
                //nldebug("<impulseDynString> Received CHAT, put in buffer : waiting association");
                return;
            }

            // diplay
            string senderName = "";
            chatDisplayer.DisplayChat(INVALID_DATASET_INDEX, ucstr, ucstr, type, 0, senderName);
        }

        internal static void FlushBuffer(IChatDisplayer chatDisplayer)
        {
            // before displaying anything, must ensure dynamic channels are up to date
            //updateDynamicChatChannels(chatDisplayer); TODO

            // **** Process waiting messages

            //ChatMsgNode itMsg;

            for (int i = 0; i < _ChatBuffer.Count; i++)
                //for (itMsg = _ChatBuffer.begin(); itMsg != _ChatBuffer.end();)
            {
                ChatMsgNode itMsg = _ChatBuffer[i];
                ChatGroupType type = itMsg.ChatMode;
                string sender = "";
                string content;

                // all strings received?
                bool complete = true;
                if (itMsg.SenderNameId != 0)
                    complete &= StringManagerClient.GetString(itMsg.SenderNameId, out sender);
                if (itMsg.UsePhraseId)
                    complete &= StringManagerClient.GetDynString(itMsg.PhraseId, out content);
                else
                    content = itMsg.Content;

                if (type == ChatGroupType.DynChat)
                {
                    //// retrieve the DBIndex from the dynamic chat id
                    //int dbIndex = ChatMngr.getDynamicChannelDbIndexFromId(itMsg->DynChatChanID);
                    //// if the client database is not yet up to date, leave the chat message in buffer
                    //if (dbIndex < 0)
                    //    complete = false;
                }

                // if complete, process
                if (complete)
                {
                    string ucstr;
                    if (itMsg.SenderNameId == 0)
                    {
                        ucstr = content;
                    }
                    else
                    {
                        if (itMsg.DisplayAsTell)
                            BuildTellSentence(sender, content, out ucstr);
                        else
                            BuildChatSentence(itMsg.CompressedIndex, sender, content, type, out ucstr);
                    }

                    // display
                    if (itMsg.DisplayAsTell)
                        chatDisplayer.DisplayTell( /*itMsg->CompressedIndex, */ucstr, sender);
                    else
                        chatDisplayer.DisplayChat(itMsg.CompressedIndex, ucstr, content, type, itMsg.DynChatChanID,
                            sender);

                    //list<ChatMsgNode>::iterator itTmp = itMsg++;
                    _ChatBuffer.Remove(itMsg);
                }

                //else
                //{
                //    ++itMsg;
                //}
            }
        }

        // ***************************************************************************
        static void BuildTellSentence(string sender, string msg, out string result)
        {
            // If no sender name was provided, show only the msg
            if (sender.Length == 0)
                result = msg;
            else
            {
                string name = Entity.RemoveTitleAndShardFromName(sender);
                string csr;

                //// special case where there is only a title, very rare case for some NPC
                //if (name.empty())
                //{
                //    // we need the gender to display the correct title
                //    CCharacterCL entity = dynamic_cast<CCharacterCL*>(EntitiesMngr.getEntityByName(sender, true, true));
                //    bool bWoman = entity && entity->getGender() == GSGENDER.female;
                //
                //    name = STRING_MANAGER.CStringManagerClient.getTitleLocalizedName(CEntityCL.getTitleFromName(sender), bWoman);
                //    {
                //        // Sometimes translation contains another title
                //        string.size_type pos = name.find('$');
                //        if (pos != string.npos)
                //        {
                //            name = STRING_MANAGER.CStringManagerClient.getTitleLocalizedName(CEntityCL.getTitleFromName(name), bWoman);
                //        }
                //    }
                //}

                //else
                //{
                // Does the char have a CSR title?
                csr = ""; // CHARACTER_TITLE.isCsrTitle(CEntityCL.getTitleFromName(sender)) ? string("(CSR) ") : string(""); TODO
                //}

                result = $"{csr}{name} tells you: {msg}";
            }
        }

        static void BuildChatSentence(uint compressedSenderIndex, string sender, string msg, ChatGroupType type,
            out string result)
        {
            // if its a tell, then use buildTellSentence
            if (type == ChatGroupType.Tell)
            {
                BuildTellSentence(sender, msg, out result);
                return;
            }

            // If no sender name was provided, show only the msg
            if (sender.Length == 0)
            {
                result = msg;
                return;
            }

            // get the category if any. Note, in some case (chat from other player), there is not categories
            // and we do not want getStringCategory to return 'SYS' category.
            string catStr = GetStringCategory(msg, out string finalMsg, false);
            string cat = "";
            if (catStr.Length > 0)
                cat = "&" + catStr + "&";

            if (cat.Length > 0)
            {
                result = msg;
                return;
            }

            // Format the sentence with the provided sender name
            string senderName = Entity.RemoveTitleAndShardFromName(sender); 

            string csr = "";
            // Does the char have a CSR title?
            //csr = CHARACTER_TITLE.isCsrTitle(CEntityCL.getTitleFromName(sender)) ? string("(CSR) ") : string("");

            //if (/*UserEntity &&*/ senderName == UserEntity->getDisplayName())
            //{
            //    // The player talks
            //    switch (type)
            //    {
            //        case ChatGroupType.Shout:
            //            result = cat + csr + CI18N.get("youShout") + string(": ") + finalMsg;
            //            break;
            //        default:
            //            result = cat + csr + CI18N.get("youSay") + string(": ") + finalMsg;
            //            break;
            //    }
            //}
            //else
            //{
            // Special case where there is only a title, very rare case for some NPC
            //if (senderName.Length == 0)
            //{
            //    CCharacterCL* entity = dynamic_cast<CCharacterCL*>(EntitiesMngr.getEntityByName(sender, true, true));
            //    // We need the gender to display the correct title
            //    bool bWoman = entity && entity->getGender() == GSGENDER.female;
            //
            //    //senderName = STRING_MANAGER.CStringManagerClient.getTitleLocalizedName(CEntityCL.getTitleFromName(sender), bWoman); todo
            //    {
            //        // Sometimes translation contains another title
            //        string.size_type pos = senderName.find('$');
            //        if (pos != string.npos)
            //        {
            //            senderName = StringManagerClient.getTitleLocalizedName(CEntityCL.getTitleFromName(senderName), bWoman);
            //        }
            //    }
            //}

            //senderName = STRING_MANAGER.CStringManagerClient.getLocalizedName(senderName); TODO
            switch (type)
            {
                case ChatGroupType.Shout:
                    result = $"{cat}{csr}{senderName} shouts: {finalMsg}";
                    break;
                default:
                    result = $"{cat}{csr}{senderName} says: {finalMsg}";
                    break;
            }

            //}
        }

        public static string GetStringCategory(string src, out string dest, bool alwaysAddSysByDefault = false)
        {
            string str = GetStringCategoryIfAny(src, out dest);
            if (alwaysAddSysByDefault)
                return str.Length == 0 ? "SYS" : str;
            else
                return str;
        }

        public static string GetStringCategoryIfAny(string src, out string dest)
        {
            char[] colorCode = new char[0];
            if (src.Length >= 3)
            {
                int startPos = 0;

                // Skip <NEW> or <CHG> if present at beginning
                string preTag = "";

                const string newTag = "<NEW>";
                if (src.Length >= PreTagSize && src.Substring(0, PreTagSize) == newTag)
                {
                    startPos = PreTagSize;
                    preTag = newTag;
                }

                const string chgTag = "<CHG>";
                if (src.Length >= PreTagSize && src.Substring(0, PreTagSize) == chgTag)
                {
                    startPos = PreTagSize;
                    preTag = chgTag;
                }

                if (src[startPos] == '&')
                {
                    int nextPos = src.IndexOf('&', startPos + 1);

                    if (nextPos != -1)
                    {
                        int codeSize = nextPos - startPos - 1;
                        colorCode = new char[codeSize];

                        for (int k = 0; k < codeSize; ++k)
                        {
                            colorCode[k] = char.ToLower(src[k + startPos + 1]);
                        }

                        string destTmp = "";
                        if (startPos != 0)
                            destTmp = preTag; // leave <NEW> or <CHG> in the dest string
                        destTmp += src.Substring(nextPos + 1);
                        dest = destTmp;
                    }
                    else
                    {
                        dest = src;
                    }
                }
                else
                {
                    dest = src;
                }
            }
            else
            {
                dest = src;
            }

            return new string(colorCode);
        }
    }
}