﻿namespace Client
{
    public static class Constants
    {
        /// <summary>
        /// game_share chat_group
        /// </summary>
        public const int MaxDynChanPerPlayer = 8;

        /// <summary>
        /// game_share base_types
        /// </summary>
        public const uint InvalidDatasetIndex = 0x00FFFFFF;

        /// <summary>
        /// game_share entity_types
        /// </summary>
        public const byte InvalidSlot = 0xFF;

        /// <summary>
        /// game_share entity_types Invalid value constant
        /// </summary>
        public const uint InvalidClientDatasetIndex = 0xFFFFF;

        /// <summary>
        /// game_share system_message
        /// </summary>
        public const int NumBitsInLongAck = 512;

        /// <remarks>
        /// This must be changed when cdbank bits in the client change
        /// </remarks>
        public const int FillNbitsWithNbBitsForCdbbank = 2;

        /// <summary>
        /// Database bank identifiers (please change CDBBankNames in cpp accordingly) - enum
        /// </summary>
        public const int CdbPlayer = 0;

        /// <summary>
        /// Number of visual properties
        /// </summary>
        public const uint NbVisualProperties = 28;

        /// <summary>
        /// Path to the file for the generic message header manager
        /// </summary>
        public const string MsgXmlPath = "./data/msg.xml";

        /// <summary>
        /// Path to the leveldesign.bnp@sheet_id.bin file
        /// </summary>
        public const string SheetsIdBinPath = "./data/sheet_id.bin";

        /// <summary>
        /// Timeout for the connection if not uninitialized or connected in ticks
        /// </summary>
        public const int ConnectionTimeout = 5 * 60; // [s] 5 minutes

        /// <summary>
        /// UDP Client Buffer
        /// </summary>
        public const int ReceiveBuffer = 64000;

        /// <summary>
        /// User agent of the Console Client (should be the newest Ryzom user agent)
        /// </summary>
        public const string UserAgent = "Ryzom/Omega / v23.07.329 #b2e8a01f6-windows-x64";

        /// <summary>
        /// Debugging for the memory stream
        /// </summary>
#if DEBUG
        public const bool BitMemoryStreamDebugEnabled = true;
#else
        public const bool BitMemoryStreamDebugEnabled = false;
#endif
    }
}
