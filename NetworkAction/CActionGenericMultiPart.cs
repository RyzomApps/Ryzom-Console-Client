using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace RCC.NetworkAction
{
    public class CActionGenericMultiPart : CActionImpulsion
    {
        byte[] PartCont;
        byte Number;
        int Part, NbBlock;

        public override void unpack(CBitMemStream message)
        {
            message.serial(ref Number);
            message.serial(ref Part);
            message.serial(ref NbBlock);

            int size = 0;
            message.serial(ref size);

            CBitMemStream part = new CBitMemStream(false);

            message.serialBuffer(message, size);

            PartCont = message.Buffer();
        }
    }
}
