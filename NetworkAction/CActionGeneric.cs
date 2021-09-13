using System;
using System.IO;
using System.Reflection;

namespace RCC.NetworkAction
{
    public class CActionGeneric : CActionImpulsion
    {
        CBitMemStream _Message;
        bool ServerSide = false;

        public override void unpack(CBitMemStream message)
        {
            // Prepare _Message for output
            _Message = new CBitMemStream(false);

            if (!_Message.isReading())
                _Message.invert();

            // Read size from message, and check to	avoid hacking!
            var size = 0;
            message.serial(ref size);
            if (size > 512 && ServerSide)
            {
                throw new InvalidDataException();
            }

            // Write the data from message to _Message
            //uint8* ptr = _Message.bufferToFill(size);
            message.serialBuffer(_Message, size);

            //message.serial (_Message);
        }

        internal CBitMemStream get()
        {
            // when we get a the message, it s that you want to read it, so change the flux if needed
            if (!_Message.isReading())
                _Message.invert();

            // reset the flux to the start
            _Message.resetBufPos();

            return _Message;
        }

        void set(CBitMemStream message)
        {
            _Message = message;

            if (!_Message.isReading())
                _Message.invert();
        }
    }
}