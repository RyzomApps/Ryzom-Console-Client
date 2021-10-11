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
using RCC.Database;
using RCC.Network;
using RCC;

namespace RCC.Chat
{
    /// <summary>
    /// Class for management of incoming and outgoing chat messages
    /// </summary>
    public class ChatManager
    {


        private readonly List<ChatMsgNode> _chatBuffer = new List<ChatMsgNode>();

        private readonly NetworkManager _networkManager;
        private readonly StringManager _stringManager;
        private readonly DatabaseManager _databaseManager;


        private readonly List<DatabaseNodeLeaf> _dynamicChannelIdLeaf = new List<DatabaseNodeLeaf>(new DatabaseNodeLeaf[Constants.MaxDynChanPerPlayer]);
        private readonly List<DatabaseNodeLeaf> _dynamicChannelNameLeaf = new List<DatabaseNodeLeaf>(new DatabaseNodeLeaf[Constants.MaxDynChanPerPlayer]);

        public ChatManager(NetworkManager networkManager, StringManager stringManager, DatabaseManager databaseManager)
        {
            _networkManager = networkManager;
            _stringManager = stringManager;
            _databaseManager = databaseManager;

            //_ChatMode = (uint8)CChatGroup::nbChatMode;
            //_ChatDynamicChannelId = 0;
            //_NumTellPeople = 0;
            //_MaxNumTellPeople = 5;

            // default to NULL
            for (int i = 0; i < Constants.MaxDynChanPerPlayer; i++)
            {
                _dynamicChannelNameLeaf[i] = null;
                _dynamicChannelIdLeaf[i] = null;
                //_DynamicChannelIdCache[i] = DynamicChannelEmptyId;
            }
        }

        /// <summary>
        /// InGame init/release. call init after init of database
        /// </summary>
        public void InitInGame()
        {
            //CInterfaceManager pIM = CInterfaceManager.getInstance();

            for (int i = 0; i < Constants.MaxDynChanPerPlayer; i++)
            {
                // default
                _dynamicChannelNameLeaf[i] = null;
                _dynamicChannelIdLeaf[i] = null;
                //_DynamicChannelIdCache[i] = DynamicChannelEmptyId;

                // get
                DatabaseNodeLeaf name = _databaseManager.GetDbProp($"SERVER:DYN_CHAT:CHANNEL{i}:NAME", false);
                DatabaseNodeLeaf id = _databaseManager.GetDbProp($"SERVER:DYN_CHAT:CHANNEL{i}:ID", false);

                if (name != null && id != null)
                {
                    _dynamicChannelNameLeaf[i] = name;
                    _dynamicChannelIdLeaf[i] = id;
                }
            }
        }

        /// <summary>
        /// interprets the incoming tell string
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
            chatDisplayer.DisplayTell(ucstr, senderStr);
        }

        /// <summary>
        /// interprets the incoming say string
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
                // TODO retrieve the DBIndex from the dynamic chat id
                int dbIndex = GetDynamicChannelDbIndexFromId(chatMsg.DynChatChanID);
                // if the client database is not yet up to date, put the chat message in buffer
                if (dbIndex < 0)
                    complete = false;
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

        /// <summary>
        /// Use info from DB SERVER:DYN_CHAT. return -1 if fails
        /// </summary>
        private int GetDynamicChannelDbIndexFromId(uint channelId)
        {
            for (int i = 0; i < Constants.MaxDynChanPerPlayer; i++)
            {
                if (_dynamicChannelIdLeaf[i] != null)
                {
                    if ((ulong)_dynamicChannelIdLeaf[i].GetValue64() == channelId)
                        return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// Extract and decode the chat string from the stream.
        /// the stream here is only a iunt32 for the id of the dynamic string
        /// </summary>
        /// <param name="type">where do you want this string to go (dyn_chat is not allowed)</param>
        internal void ProcessChatStringWithNoSender(BitMemoryStream bms, ChatGroupType type, IChatDisplayer chatDisplayer)
        {
            Debug.Assert(type != ChatGroupType.DynChat);

            // serial
            var chatMsg = new ChatMsg2();
            uint phraseID = 0;
            bms.Serial(ref phraseID);

            chatMsg.CompressedIndex = Constants.InvalidDatasetIndex;
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

            // display
            const string senderName = "";
            chatDisplayer.DisplayChat(Constants.InvalidDatasetIndex, ucstr, ucstr, type, 0, senderName);
        }

        /// <summary>
        /// Process waiting messages
        /// </summary>
        internal void FlushBuffer(IChatDisplayer chatDisplayer)
        {
            // before displaying anything, must ensure dynamic channels are up to date
            // TODO updateDynamicChatChannels(chatDisplayer); 

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
                    // retrieve the DBIndex from the dynamic chat id
                    int dbIndex = GetDynamicChannelDbIndexFromId(itMsg.DynChatChanID);
                    // if the client database is not yet up to date, leave the chat message in buffer
                    if (dbIndex < 0)
                        complete = false;
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
                        chatDisplayer.DisplayTell(ucstr, sender);
                    else
                        chatDisplayer.DisplayChat(itMsg.CompressedIndex, ucstr, content, type, itMsg.DynChatChanID, sender);

                    _chatBuffer.Remove(itMsg);
                }
            }
        }

        /// <summary>
        /// build a sentence to be displayed in the tell
        /// </summary>
        private static void BuildTellSentence(string sender, string msg, out string result)
        {
            // If no sender name was provided, show only the msg
            var name = Entity.Entity.RemoveTitleAndShardFromName(sender);

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

        /// <summary>
        /// build a sentence to be displayed in the chat (e.g add "you say", "you shout", "[user name] says" or "[user name] shout")
        /// </summary>
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
            var senderName = Entity.Entity.RemoveTitleAndShardFromName(sender);

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

        /// <summary>
        /// get the category if any. Note, in some case (chat from other player), there is not categories
        /// and we do not want getStringCategory to return 'SYS' category.
        /// </summary>
        public static string GetStringCategory(string src, out string dest, bool alwaysAddSysByDefault = false)
        {
            var str = GetStringCategoryIfAny(src, out dest);

            if (alwaysAddSysByDefault)
                return str.Length == 0 ? "SYS" : str;

            return str;
        }

        /// <summary>
        /// Get the category from the string (src="&SYS&Who are you?" and dest="Who are you?" and return "SYS"), if no category, return ""
        /// </summary>
        public static string GetStringCategoryIfAny(string src, out string dest)
        {
            const int PreTagSize = 5;

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

                        destTmp += src[(nextPos + 1)..];
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