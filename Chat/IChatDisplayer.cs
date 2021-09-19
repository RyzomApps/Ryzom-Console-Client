using RCC.Client;

namespace RCC.Chat
{
    public interface IChatDisplayer
    {
        /// <param name="mode">in which channel should this message goes</param>
        /// <param name="dynChatId">is valid only if mode==dyn_chat. This the Id of channel (not the index in DB!)</param>
        public void DisplayChat(uint compressedSenderIndex, string ucstr, string rawMessage, ChatGroupType mode, uint dynChatId, string senderName, uint bubbleTimer = 0);

        /// <summary>
        /// display a player tell message
        /// </summary>
        public void DisplayTell(/*TDataSetIndex senderIndex, */string ucstr, string senderName);

        /// <summary>
        /// Clear a channel.
        /// </summary>
        /// <param name="dynChatDbIndex">is valid only if mode==dyn_chat. Contrary to displayChat, this is the Db Index (0..MaxDynChanPerPlayer)</param>
        public void ClearChannel(ChatGroupType mode, uint dynChatDbIndex);
    };
}