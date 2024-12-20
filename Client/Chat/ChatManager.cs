﻿///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Diagnostics;
using API.Chat;
using Client.Database;
using Client.Messages;
using Client.Network;
using Client.Stream;
using Client.Strings;

namespace Client.Chat
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

        public ChatManager(NetworkManager networkManager)
        {
            _networkManager = networkManager;
            _stringManager = networkManager.GetStringManager();
            _databaseManager = networkManager.GetDatabaseManager();

            // default to NULL
            for (var i = 0; i < Constants.MaxDynChanPerPlayer; i++)
            {
                _dynamicChannelIdLeaf[i] = null;
            }
        }

        /// <summary>
        /// InGame init/release. call init after init of database
        /// </summary>
        public void InitInGame()
        {
            for (var i = 0; i < Constants.MaxDynChanPerPlayer; i++)
            {
                // default
                _dynamicChannelIdLeaf[i] = null;

                // get
                var name = _databaseManager?.GetServerNode($"SERVER:DYN_CHAT:CHANNEL{i}:NAME", false);
                var id = _databaseManager?.GetServerNode($"SERVER:DYN_CHAT:CHANNEL{i}:ID", false);

                if (name != null && id != null)
                {
                    _dynamicChannelIdLeaf[i] = id;
                }
            }
        }

        /// <summary>
        /// Change the chat mode
        /// </summary>
        /// <param name="group">Is the chat mode( say/shout/group/clade )</param>
        /// <param name="dynamicChannelId">The dynamic channel id (if group==dyn_chat).
        /// Not the db index! Use getDynamicChannelIdFromDbIndex() if you got a dbIndex</param>
        public void SetChatMode(ChatGroupType group, uint dynamicChannelId = 118095872u)
        {
            var mode = (byte)group;

            // Chat team don't need swap mode
            if (group == ChatGroupType.Team)
                return;

            var bms = new BitMemoryStream();
            const string msgType = "STRING:CHAT_MODE";

            if (_networkManager.GetMessageHeaderManager().PushNameToStream(msgType, bms))
            {
                bms.Serial(ref mode);
                bms.Serial(ref dynamicChannelId, 64);
                _networkManager.Push(bms);
                //RyzomClient.GetInstance().GetLogger().Debug($"impulseCallBack : {msgType} {mode} sent");
            }
            else
            {
                RyzomClient.GetInstance().GetLogger().Warn($"Unknown message named '{msgType}'.");
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
            ChatManagerHelper.BuildTellSentence(senderStr, chatMsg.Content, out var ucstr);
            chatDisplayer.DisplayTell(ucstr, senderStr, chatMsg.Content);
        }

        /// <summary>
        /// Extract and decode the chat string from the stream. display now if ready or delay in flushBuffer()
        /// </summary>
        public void ProcessChatString(BitMemoryStream bms, RyzomClient chatDisplayer)
        {
            // before displaying anything, must ensure dynamic channels are up to date
            // NB: only processChatString() have to do this. Other methods cannot be in dyn_chat mode

            // TODO UpdateDynamicChatChannels(chatDisplayer) for ProcessChatString

            // serial
            var chatMsg = new ChatMsg();
            chatMsg.Serial(bms);

            var type = chatMsg.ChatMode;

            var complete = _stringManager.GetString(chatMsg.SenderNameId, out var senderStr, _networkManager);

            if (type == ChatGroupType.DynChat)
            {
                // TODO retrieve the DBIndex from the dynamic chat id
                var dbIndex = GetDynamicChannelDbIndexFromId(chatMsg.DynChatChanID);

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
            ChatManagerHelper.BuildChatSentence(chatDisplayer, senderStr, chatMsg.Content, type, out var ucstr);

            // Send to the displayers
            chatDisplayer.DisplayChat(chatMsg.CompressedIndex, ucstr, chatMsg.Content, type, chatMsg.DynChatChanID, senderStr);
        }

        /// <summary>
        /// Extract and decode the chat string from the stream. display now if ready or delay in flushBuffer()
        /// Difference with processChatString() is that processChatString2() receive a DynamicString for the message
        /// </summary>
        public void ProcessChatString2(BitMemoryStream bms, RyzomClient chatDisplayer)
        {
            var chatMsg = new ChatMsg2();
            chatMsg.Serial(bms);

            var type = chatMsg.ChatMode;

            // here, the type cannot be dyn_chat (no DynChatId in the message) => discard
            if (type == ChatGroupType.DynChat)
            {
                chatDisplayer.GetLogger().Warn($"Client does not support dynamic chat with messages of type 2: '{chatMsg.PhraseId}' aborted.");
                return;
            }

            // if !complete, wait
            var complete = true;
            complete &= _stringManager.GetString(chatMsg.SenderNameId, out var senderStr, _networkManager);
            complete &= _stringManager.GetDynString(chatMsg.PhraseId, out var rawMessage, _networkManager);

            if (!complete)
            {
                _chatBuffer.Add(new ChatMsgNode(chatMsg, false));
                return;
            }

            rawMessage += " ";
            rawMessage += chatMsg.CustomTxt;

            // display
            ChatManagerHelper.BuildChatSentence(chatDisplayer, senderStr, rawMessage, type, out var ucstr);
            chatDisplayer.DisplayChat(chatMsg.CompressedIndex, ucstr, rawMessage, type, 0xFFFFF, senderStr);
        }

        /// <summary>
        /// Use info from DB SERVER:DYN_CHAT
        /// </summary>
        /// <returns>-1 if fails</returns>
        private int GetDynamicChannelDbIndexFromId(uint channelId)
        {
            for (var i = 0; i < Constants.MaxDynChanPerPlayer; i++)
            {
                if (_dynamicChannelIdLeaf[i] == null) continue;

                if ((ulong)_dynamicChannelIdLeaf[i].GetValue64() == channelId)
                    return i;
            }

            return -1;
        }

        /// <summary>
        /// Extract and decode the chat string from the stream.
        /// the stream here is only a iunt32 for the id of the dynamic string
        /// </summary>
        /// <param name="bms">memory stream</param>
        /// <param name="type">where do you want this string to go (dyn_chat is not allowed)</param>
        /// <param name="chatDisplayer">class that acts as the display</param>
        internal void ProcessChatStringWithNoSender(BitMemoryStream bms, ChatGroupType type, IChatDisplayer chatDisplayer)
        {
            Debug.Assert(type != ChatGroupType.DynChat);

            // serial
            var chatMsg = new ChatMsg2();
            uint phraseId = 0;
            bms.Serial(ref phraseId);

            chatMsg.CompressedIndex = Constants.InvalidDatasetIndex;
            chatMsg.SenderNameId = 0;
            chatMsg.ChatMode = type;
            chatMsg.PhraseId = phraseId;

            // if !complete, wait
            var complete = _stringManager.GetDynString(chatMsg.PhraseId, out var ucstr, _networkManager);

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
        internal void FlushBuffer(RyzomClient chatDisplayer)
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
                    var dbIndex = GetDynamicChannelDbIndexFromId(itMsg.DynChatChanID);

                    // if the client database is not yet up to date, leave the chat message in buffer
                    if (dbIndex < 0)
                        complete = false;
                }

                // if complete, process
                if (!complete) continue;

                string ucstr;

                if (itMsg.SenderNameId == 0)
                {
                    ucstr = content;
                }
                else
                {
                    if (itMsg.DisplayAsTell)
                        ChatManagerHelper.BuildTellSentence(sender, content, out ucstr);
                    else
                        ChatManagerHelper.BuildChatSentence(chatDisplayer, sender, content, type, out ucstr);
                }

                // display
                if (itMsg.DisplayAsTell)
                    chatDisplayer.DisplayTell(ucstr, sender, content);
                else
                    chatDisplayer.DisplayChat(itMsg.CompressedIndex, ucstr, content, type, itMsg.DynChatChanID, sender);

                _chatBuffer.Remove(itMsg);
            }
        }
    }
}