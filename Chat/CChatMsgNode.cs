namespace RCC.Chat
{
    public class CChatMsgNode
    {
        public uint CompressedIndex;
        public uint SenderNameId;
        public ChatGroupType ChatMode;
        public uint DynChatChanID;
        // For Chat and Tell messages
        public string Content;
        // For Chat2 and Tell2 messages
        public uint PhraseId;
        // Use PhraseId or Content?
        public bool UsePhraseId;
        // displayTell() or displayChat()
        public bool DisplayAsTell;

        public CChatMsgNode(CChatMsg chatMsg, bool displayAsTell)
        {
            CompressedIndex= chatMsg.CompressedIndex;
        	SenderNameId= chatMsg.SenderNameId;
        	ChatMode= chatMsg.ChatMode;
        	DynChatChanID= chatMsg.DynChatChanID;
        	Content= chatMsg.Content;
        	PhraseId= 0;
        	UsePhraseId= false;
        	DisplayAsTell= displayAsTell;
        }

        public CChatMsgNode(CChatMsg2 chatMsg, bool displayAsTell)
        {
            CompressedIndex = chatMsg.CompressedIndex;
            SenderNameId = chatMsg.SenderNameId;
            ChatMode = chatMsg.ChatMode;
            DynChatChanID = 0;
            PhraseId = chatMsg.PhraseId;
            UsePhraseId = true;
            DisplayAsTell = displayAsTell;
        }
    };
}