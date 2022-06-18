///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

using System;
using System.Diagnostics;

namespace Client.Network.Action
{
    /// <summary>
    /// represents an action that is splitted into serveral parts for transport
    /// </summary>
    public class ActionGenericMultiPart : ActionImpulsion
    {
        public short NbBlock;
        public byte Number;
        public short Part;
        public byte[] PartCont = new byte[0];

        /// <summary>
        /// unpack the action from the stream
        /// </summary>
        /// <param name="message">bit stream</param>
        public override void Unpack(BitMemoryStream message)
        {
            message.Serial(ref Number);
            message.Serial(ref Part);
            message.Serial(ref NbBlock);

            var size = 0;
            message.Serial(ref size);

            PartCont = new byte[size];

            message.Serial(ref PartCont);
        }

        /// <summary>
        /// This method intialises the action with a default state
        /// </summary>
        public override void Reset()
        {
            PartCont = new byte[0];
            AllowExceedingMaxSize = false;
        }


        /// <summary>
        /// Returns the size of this action when it will be send to the UDP connection:
        /// the size is IN BITS, not in bytes(the actual size is this one plus the header size)
        /// </summary>
        public override int Size()
        {
            // header
            var bytesize = 1 + 2 + 2 + 4;
            bytesize += PartCont.Length;
            return bytesize * 8;
        }

        /// <summary>
        /// pack a message for the stream
        /// </summary>
        public override void Pack(BitMemoryStream message)
        {
            message.Serial(ref Number);
            message.Serial(ref Part);
            message.Serial(ref NbBlock);

            // Workaround for SerialCont
            var size = PartCont.Length;
            message.Serial(ref size);

            message.Serial(ref PartCont);
        }

        /// <summary>
        /// uint8* version (to match with sendImpulsion() optimisation) (size are in BYTES)
        /// </summary>
        /// <remarks>Preconditions: size != 0</remarks>
        public void Set(in byte number, short part, byte[] buffer, int bytelen, int size, short nbBlock)
        {
            Debug.Assert(size != 0);

            Reset();

            var start = part * size;
            var end = start + size;

            if (end > bytelen)
            {
                end = bytelen;
            }

            PartCont = new byte[end - start];

            Array.Copy(buffer, start, PartCont, 0, end - start);

            Number = number;
            Part = part;
            NbBlock = nbBlock;
        }
    }
}