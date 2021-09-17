// This code is a modified version of a file from the 'Ryzom - MMORPG Framework'
// <http://dev.ryzom.com/projects/ryzom/>,
// which is released under GNU Affero General Public License.
// <http://www.gnu.org/licenses/>
// Original Copyright 2010 by Winch Gate Property Limited

using System;
using RCC.Network;

namespace RCC.NetworkAction
{
    public class ActionGenericMultiPart : ActionImpulsion
    {
        public short NbBlock;
        public byte Number;
        public short Part;
        public byte[] PartCont;

        public override void unpack(BitMemoryStream message)
        {
            //Debug.Print(message.ToString());

            message.Serial(ref Number);
            message.Serial(ref Part);
            message.Serial(ref NbBlock);

            int size = 0;
            message.Serial(ref size);

            PartCont = new byte[size];

            message.Serial(ref PartCont);

            //PartCont = PartCont.Reverse().ToArray();

            //var part = new CBitMemStream(false);
            //
            //part.serialBuffer(message, size);
            //
            //PartCont = new List<byte>(part.Buffer());
        }

        /// <summary>
        ///     Returns the size of this action when it will be send to the UDP connection:
        ///     the size is IN BITS, not in bytes(the actual size is this one plus the header size)
        /// </summary>
        /// <returns></returns>
        public override int size()
        {
            int bytesize = 1 + 2 + 2 + 4; // header
            bytesize += PartCont.Length;
            return bytesize * 8;
        }

        public override void pack(BitMemoryStream message)
        {
            throw new NotImplementedException();

            //message.serial(ref Number);
            //message.serial(ref Part);
            //message.serial(ref NbBlock);
            //message.serialCont(PartCont);
        }
    }
}