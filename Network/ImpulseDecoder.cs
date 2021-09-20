///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using RCC.NetworkAction;

namespace RCC.Network
{
    /// <summary>
    ///     Interpretation of actions deserialized from a stream
    /// </summary>
    internal static class ImpulseDecoder
    {
        private static readonly int[] LastAck0 = new int[1];
        private static readonly int[] LastAck1 = new int[2];
        private static readonly int[] LastAck2 = new int[4];

        /// <summary>
        ///     Constructor
        /// </summary>
        static ImpulseDecoder()
        {
            Reset();
        }

        /// <summary>
        ///     clear the decoder for a new incoming packet
        /// </summary>
        public static void Reset()
        {
            uint i;
            for (i = 0; i < 1; ++i) LastAck0[i] = -1;
            for (i = 0; i < 2; ++i) LastAck1[i] = -1;
            for (i = 0; i < 4; ++i) LastAck2[i] = -1;
        }

        /// <summary>
        ///     unpacking actions from a stream and calling the corresponding impusions
        /// </summary>
        public static void Decode(BitMemoryStream inbox, int receivedPacket, int receivedAck, int nextSentPacket,
            List<Action> actions)
        {
            uint level;

            for (level = 0; level < 3; ++level)
            {
                var lAck = new int[0];
                uint channel = 0;

                switch (level)
                {
                    case 0:
                        lAck = LastAck0;
                        channel = 0;
                        break;
                    case 1:
                        lAck = LastAck1;
                        channel = (uint) (receivedPacket & 1);
                        break;
                    case 2:
                        lAck = LastAck2;
                        channel = (uint) (receivedPacket & 3);
                        break;
                }

                var keep = true;
                var checkOnce = false;
                uint num = 0;

                var lastAck = lAck[channel];

                for (;;)
                {
                    var next = false;
                    inbox.Serial(ref next);

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
                    var action = ActionFactory.Unpack(inbox, false);

                    if (action == null)
                    {
                        continue;
                    }

                    if (keep)
                    {
                        actions.Add(action);
                        RyzomClient.Log?.Debug(
                            $"CLIMPD: received new impulsion {action.Code} (len={ActionFactory.Size(action)}) at level {level} (channel {channel})");
                    }
                    else
                    {
                        RyzomClient.Log?.Warn(
                            $"CLIMPD: discarded action {action.Code} (len={ActionFactory.Size(action)}) at level {level} (channel {channel})");
                        ActionFactory.Remove(action);
                    }
                }

                if (checkOnce)
                {
                    RyzomClient.Log?.Debug(
                        $"CLIMPD: at level {level} (channel {channel}), {num} actions{(keep ? "" : " (discarded)")} (ReceivedAck={receivedAck}/lastAck={lastAck}/nextSentPacket={nextSentPacket})");
                }
            }
        }
    }
}