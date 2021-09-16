using System.Collections.Generic;
using System.Diagnostics;
using RCC.Helper;
using RCC.NetworkAction;

namespace RCC.Network
{
    static class CImpulseDecoder
    {
        private static int[] _LastAck0 = new int[1];
        private static int[] _LastAck1 = new int[2];
        private static int[] _LastAck2 = new int[4];


        /// <summary>
        /// Constructor
        /// </summary>
        static CImpulseDecoder()
        {
            reset();
        }

        public static void decode(CBitMemStream inbox, int receivedPacket, int receivedAck, int nextSentPacket, List<CAction> actions)
        {
            uint level;

            for (level = 0; level < 3; ++level)
            {
                var lAck = new int[0];
                uint channel = 0;

                switch (level)
                {
                    case 0: lAck = _LastAck0; channel = 0; break;
                    case 1: lAck = _LastAck1; channel = (uint)(receivedPacket & 1); break;
                    case 2: lAck = _LastAck2; channel = (uint)(receivedPacket & 3); break;
                }

                var keep = true;
                var checkOnce = false;
                uint num = 0;

                var lastAck = lAck[channel];

                for (; ; )
                {
                    var next = false;
                    inbox.serial(ref next);

                    if (!next)
                        break;

                    if (!checkOnce)
                    {
                        checkOnce = true;
                        keep = receivedAck >= lAck[channel];
                        if (keep)
                            lAck[channel] = nextSentPacket;
                    }

                    ++num;
                    var action = CActionFactory.unpack(inbox, false);

                    if (action == null)
                    {
                        continue;
                    }

                    if (keep)
                    {
                        actions.Add(action);
                        //ConsoleIO.WriteLine($"CLIMPD: received new impulsion {action.Code} (len={CActionFactory.size(action)}) at level {level} (channel {channel})");
                    }
                    else
                    {
                        ConsoleIO.WriteLine($"§cCLIMPD: discarded action {action.Code} (len={CActionFactory.size(action)}) at level {level} (channel {channel})");
                        CActionFactory.remove(action);
                    }
                }

                if (checkOnce)
                {
                    //ConsoleIO.WriteLine($"CLIMPD: at level {level} (channel {channel}), {num} actions{(keep ? "" : " (discarded)")} (ReceivedAck={receivedAck}/lastAck={lastAck}/nextSentPacket={nextSentPacket})");
                }
            }
        }

        public static void reset()
        {
            uint i;
            for (i = 0; i < 1; ++i) _LastAck0[i] = -1;
            for (i = 0; i < 2; ++i) _LastAck1[i] = -1;
            for (i = 0; i < 4; ++i) _LastAck2[i] = -1;
        }
    }
}
