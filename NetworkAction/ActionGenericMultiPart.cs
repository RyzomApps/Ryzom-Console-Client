using System;
using RCC.Network;

namespace RCC.NetworkAction
{
    /// <summary>
    ///     represents an action that is splitted into serveral parts for transport
    /// </summary>
    public class ActionGenericMultiPart : ActionImpulsion
    {
        public short NbBlock;
        public byte Number;
        public short Part;
        public byte[] PartCont;

        /// <summary>
        ///     unpack the action from the stream
        /// </summary>
        /// <param name="message"></param>
        public override void Unpack(BitMemoryStream message)
        {
            message.Serial(ref Number);
            message.Serial(ref Part);
            message.Serial(ref NbBlock);

            int size = 0;
            message.Serial(ref size);

            PartCont = new byte[size];

            message.Serial(ref PartCont);
        }

        /// <summary>
        ///     Returns the size of this action when it will be send to the UDP connection:
        ///     the size is IN BITS, not in bytes(the actual size is this one plus the header size)
        /// </summary>
        public override int Size()
        {
            var bytesize = 1 + 2 + 2 + 4; // header
            bytesize += PartCont.Length;
            return bytesize * 8;
        }

        /// <summary>
        ///     pack a message for the stream
        /// </summary>
        public override void Pack(BitMemoryStream message)
        {
            throw new NotImplementedException();

            //message.serial(ref Number);
            //message.serial(ref Part);
            //message.serial(ref NbBlock);
            //message.serialCont(PartCont);
        }
    }
}