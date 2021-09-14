using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using RCC.Network;

namespace RCC.NetworkAction
{
    public class CActionGenericMultiPart : CActionImpulsion
    {
        public byte[] PartCont;
        public byte Number;
        public short Part;
        public short NbBlock;

        public override void unpack(CBitMemStream message)
        {
            Debug.Print(message.ToString());

            message.serial(ref Number);
            message.serial(ref Part);
            message.serial(ref NbBlock);

            int size = 0;
            message.serial(ref size);

            PartCont = new byte[size];

            message.serial(ref PartCont);

            PartCont = PartCont.Reverse().ToArray();

            //var part = new CBitMemStream(false);
            //
            //part.serialBuffer(message, size);
            //
            //PartCont = new List<byte>(part.Buffer());
        }

        /// <summary>
        ///  Returns the size of this action when it will be send to the UDP connection:
        /// the size is IN BITS, not in bytes(the actual size is this one plus the header size)
        /// </summary>
        /// <returns></returns>
        public override int size()
        {
            int bytesize = 1 + 2 + 2 + 4;    // header
            bytesize += PartCont.Length;
            return bytesize * 8;
        }

        public override void pack(CBitMemStream message)
        {
            throw new NotImplementedException();

            //message.serial(ref Number);
            //message.serial(ref Part);
            //message.serial(ref NbBlock);
            //message.serialCont(PartCont);
        }
    }
}
