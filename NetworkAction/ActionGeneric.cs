using System.IO;
using RCC.Network;

namespace RCC.NetworkAction
{
    /// <summary>
    ///     generic action that was sent over the network connection
    /// </summary>
    public class ActionGeneric : ActionImpulsion
    {
        private const bool ServerSide = false;
        private BitMemoryStream _message;

        /// <summary>
        ///     unpack the message from the stream
        /// </summary>
        public override void Unpack(BitMemoryStream message)
        {
            // Prepare _Message for output
            _message = new BitMemoryStream(false, 0);

            // Read size from message, and check to	avoid hacking!
            var size = 0;
            message.Serial(ref size);

            if (size > 512 && ServerSide)
            {
                throw new InvalidDataException();
            }

            // Write the data from message to _Message
            message.SerialBuffer(_message, size);
        }

        /// <summary>
        ///     returns the corresponding stream
        /// </summary>
        internal BitMemoryStream Get()
        {
            // when we get a the message, it s that you want to read it, so change the flux if needed
            if (!_message.IsReading())
                _message.Invert();

            // reset the flux to the start
            //_Message.resetBufPos(); <- this would reset the stream, but we are using the stream instead of the buffer so this is not good

            return _message;
        }

        /// <summary>
        ///     sets the corresponding stream
        /// </summary>
        internal void Set(BitMemoryStream message)
        {
            _message = message;

            if (!_message.IsReading())
                _message.Invert();
        }


        /// <summary>
        ///     Returns the size of this action when it will be send to the UDP connection:
        ///     the size is IN BITS, not in bytes(the actual size is this one plus the header size)
        /// </summary>
        public override int Size()
        {
            // If you change this size, please update IMPULSE_ACTION_HEADER_SIZE in the front-end

            // in bits!!! (the message size and after the message itself)
            return (4 + (_message?.Length ?? 0)) * 8;
        }

        /// <summary>
        ///     pack a message for the stream
        /// </summary>
        public override void Pack(BitMemoryStream message)
        {
            //byte[] obj = _Message.Buffer();
            //message.serial(ref obj);
            message.SerialBufferWithSize(_message.Buffer(), _message.Buffer().Length);
            //throw new System.NotImplementedException();
        }
    }
}