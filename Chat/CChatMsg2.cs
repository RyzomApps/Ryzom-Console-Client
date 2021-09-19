﻿using RCC.Network;

namespace RCC.Chat
{
    public class CChatMsg2
    {
        public uint CompressedIndex;
        public uint SenderNameId;
        public ChatGroupType ChatMode;
        public uint PhraseId;
        public string CustomTxt;

        public CChatMsg2()
        {
            CompressedIndex = ChatManager.INVALID_DATASET_INDEX;
            SenderNameId = 0;
            ChatMode = 0;
            PhraseId = 0;
            CustomTxt = "";
        }

        public void Serial(BitMemoryStream f)
        {
            f.Serial(ref CompressedIndex);
            f.Serial(ref SenderNameId);
            byte ChatModeByte = 0;
            f.Serial(ref ChatModeByte);
            ChatMode = (ChatGroupType)ChatModeByte;
            f.Serial(ref PhraseId);
            f.Serial(ref CustomTxt);
        }
    };
}