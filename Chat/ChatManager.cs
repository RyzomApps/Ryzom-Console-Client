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
    internal class ChatManager
    {

        public const uint InvalidDatasetIndex = 0x00FFFFFF;

        private const int PreTagSize = 5;
        private readonly List<ChatMsgNode> _chatBuffer = new List<ChatMsgNode>();

        private readonly NetworkManager _networkManager;
        private readonly StringManager _stringManager;

        public ChatManager(NetworkManager networkManager, StringManager stringManager)
        {
            _networkManager = networkManager;
            _stringManager = stringManager;
        }

        /// <summary>
        ///     interprets the incoming tell string
        /// </summary>
        public void ProcessTellString(BitMemoryStream bms, IChatDisplayer chatDisplayer)
        {
            var chatMsg = new ChatMsg();

            // Serial. For tell message, there is no chat mode, coz we know we are in tell mode !
            bms.Serial(ref chatMsg.CompressedIndex);
            bms.Serial(ref chatMsg.SenderNameId);
            bms.Serial(ref chatMsg.Content);

            chatMsg.ChatMode = ChatGroupType.Tell;

            // If !complete, wait
            var complete = _stringManager.GetString(chatMsg.SenderNameId, out var senderStr, _networkManager);

            if (!complete)
            {
                _chatBuffer.Add(new ChatMsgNode(chatMsg, true));
                RyzomClient.GetInstance().GetLogger().Debug("<impulseTell> Received TELL, put in buffer : waiting association");
                return;
            }

            // display
            BuildTellSentence(senderStr, chatMsg.Content, out var ucstr);
            chatDisplayer.DisplayTell( ucstr, senderStr);
        }

        /// <summary>
        ///     interprets the incoming say string
        /// </summary>
        public void ProcessChatString(BitMemoryStream bms, IChatDisplayer chatDisplayer)
        {
            // before displaying anything, must ensure dynamic channels are up to date
            // NB: only processChatString() have to do this. Other methods cannot be in dyn_chat mode
            // TODO updateDynamicChatChannels(chatDisplayer); // in ProcessChatString

            // serial
            var chatMsg = new ChatMsg();
            chatMsg.Serial(bms);

            var type = chatMsg.ChatMode;

            var complete = _stringManager.GetString(chatMsg.SenderNameId, out var senderStr, _networkManager);

            if (type == ChatGroupType.DynChat)
            {
                //// TODO retrieve the DBIndex from the dynamic chat id
                //sint32 dbIndex = ChatManager.getDynamicChannelDbIndexFromId(chatMsg.DynChatChanID); 
                //// if the client database is not yet up to date, put the chat message in buffer
                //if (dbIndex < 0)
                //    complete = false;
            }

            // if !complete, wait
            if (!complete)
            {
                _chatBuffer.Add(new ChatMsgNode(chatMsg, false));
                return;
            }

            // display
            BuildChatSentence(senderStr, chatMsg.Content, type, out var ucstr);
            chatDisplayer.DisplayChat(chatMsg.CompressedIndex, ucstr, chatMsg.Content, type, chatMsg.DynChatChanID,
                senderStr);
        }

        internal void ProcessChatStringWithNoSender(BitMemoryStream bms, ChatGroupType type, IChatDisplayer chatDisplayer)
        {
            Debug.Assert(type != ChatGroupType.DynChat);

            // serial
            var chatMsg = new ChatMsg2();
            uint phraseID = 0;
            bms.Serial(ref phraseID);

            chatMsg.CompressedIndex = InvalidDatasetIndex;
            chatMsg.SenderNameId = 0;
            chatMsg.ChatMode = type;
            chatMsg.PhraseId = phraseID;

            // if !complete, wait
            var complete = _stringManager.GetDynString(chatMsg.PhraseId, out string ucstr, _networkManager);

            if (!complete)
            {
                _chatBuffer.Add(new ChatMsgNode(chatMsg, false));
                return;
            }

            // diplay
            const string senderName = "";
            chatDisplayer.DisplayChat(InvalidDatasetIndex, ucstr, ucstr, type, 0, senderName);
        }

        internal void FlushBuffer(IChatDisplayer chatDisplayer)
        {
            // before displaying anything, must ensure dynamic channels are up to date
            // TODO updateDynamicChatChannels(chatDisplayer); 

            // **** Process waiting messages
            for (var i = 0; i < _chatBuffer.Count; i++)
            {
                var itMsg = _chatBuffer[i];
                var type = itMsg.ChatMode;
                var sender = "";
                string content;

                // all strings received?
                var complete = true;

                if (itMsg.SenderNameId != 0)
                    complete &= _stringManager.GetString(itMsg.SenderNameId, out sender, _networkManager);
                if (itMsg.UsePhraseId)
                    complete &= _stringManager.GetDynString(itMsg.PhraseId, out content, _networkManager);
                else
                    content = itMsg.Content;

                if (type == ChatGroupType.DynChat)
                {
                    //// TODO retrieve the DBIndex from the dynamic chat id
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
                            BuildChatSentence(sender, content, type, out ucstr);
                    }

                    // display
                    if (itMsg.DisplayAsTell)
                        chatDisplayer.DisplayTell( /*itMsg->CompressedIndex, */ucstr, sender);
                    else
                        chatDisplayer.DisplayChat(itMsg.CompressedIndex, ucstr, content, type, itMsg.DynChatChanID,
                            sender);

                    //list<ChatMsgNode>::iterator itTmp = itMsg++;
                    _chatBuffer.Remove(itMsg);
                }
            }
        }

        private static void BuildTellSentence(string sender, string msg, out string result)
        {
            // If no sender name was provided, show only the msg
            var name = Entity.RemoveTitleAndShardFromName(sender);

            if (sender.Length == 0)
                result = msg;
            else
            {
                // TODO special case where there is only a title, very rare case for some NPC

                // TODO Does the char have a CSR title?
                const string csr = "";

                result = $"{csr}{name} tells you: {msg}";
            }
        }

        private static void BuildChatSentence(string sender, string msg, ChatGroupType type,
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
            var catStr = GetStringCategory(msg, out var finalMsg);
            var cat = "";

            if (catStr.Length > 0)
                cat = "&" + catStr + "&";

            if (cat.Length > 0)
            {
                result = msg;
                return;
            }

            // Format the sentence with the provided sender name
            var senderName = Entity.RemoveTitleAndShardFromName(sender);

            // TODO Does the char have a CSR title?
            const string csr = "";

            // TODO: The player talks -> show as you say:

            // TODO: Special case where there is only a title, very rare case for some NPC

            // TODO senderName = STRING_MANAGER.CStringManagerClient.getLocalizedName(senderName); 

            result = type switch
            {
                ChatGroupType.Shout => $"{cat}{csr}{senderName} shouts: {finalMsg}",
                _ => $"{cat}{csr}{senderName} says: {finalMsg}"
            };
        }

        public static string GetStringCategory(string src, out string dest, bool alwaysAddSysByDefault = false)
        {
            var str = GetStringCategoryIfAny(src, out dest);

            if (alwaysAddSysByDefault)
                return str.Length == 0 ? "SYS" : str;

            return str;
        }

        public static string GetStringCategoryIfAny(string src, out string dest)
        {
            var colorCode = new char[0];

            if (src.Length >= 3)
            {
                var startPos = 0;

                // Skip <NEW> or <CHG> if present at beginning
                var preTag = "";

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
                    var nextPos = src.IndexOf('&', startPos + 1);

                    if (nextPos != -1)
                    {
                        var codeSize = nextPos - startPos - 1;
                        colorCode = new char[codeSize];

                        for (var k = 0; k < codeSize; ++k)
                        {
                            colorCode[k] = char.ToLower(src[k + startPos + 1]);
                        }

                        var destTmp = "";

                        if (startPos != 0)
                            // leave <NEW> or <CHG> in the dest string
                            destTmp = preTag; 

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