using System.Diagnostics;
using System.IO;
using RCC.Network;

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
            Debug.WriteLine(_Message);
        }

        internal CBitMemStream get()
        {
            // when we get a the message, it s that you want to read it, so change the flux if needed
            if (!_Message.isReading())
                _Message.invert();

            // reset the flux to the start
            //_Message.resetBufPos(); <- this would reset the stream, but we are using the stream instead of the buffer so this is not good

            return _Message;
        }

        void set(CBitMemStream message)
        {
            _Message = message;

            if (!_Message.isReading())
                _Message.invert();
        }

        public override int size()
        {
            // If you change this size, please update IMPULSE_ACTION_HEADER_SIZE in the front-end

            // in bits!!! (the message size and after the message itself)
            return (4 + _Message.Length) * 8;
        }
    }
}