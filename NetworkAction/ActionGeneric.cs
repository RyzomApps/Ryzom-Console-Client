// This code is a modified version of a file from the 'Ryzom - MMORPG Framework'
// <http://dev.ryzom.com/projects/ryzom/>,
// which is released under GNU Affero General Public License.
// <http://www.gnu.org/licenses/>
// Original Copyright 2010 by Winch Gate Property Limited

using System.IO;
using RCC.Network;

namespace RCC.NetworkAction
{
    public class ActionGeneric : ActionImpulsion
    {
        BitMemoryStream _Message;
        readonly bool ServerSide = false;

        public override void unpack(BitMemoryStream message)
        {
            // Prepare _Message for output
            _Message = new BitMemoryStream(false, 0);

            //if (!_Message.isReading())
            //    _Message.invert();

            // Read size from message, and check to	avoid hacking!
            var size = 0;
            message.Serial(ref size);

            if (size > 512 && ServerSide)
            {
                throw new InvalidDataException();
            }

            // Write the data from message to _Message
            //uint8* ptr = _Message.bufferToFill(size);
            message.SerialBuffer(_Message, size);

            //message.serial (_Message);
            //Debug.Print(_Message.ToString());
        }

        internal BitMemoryStream get()
        {
            // when we get a the message, it s that you want to read it, so change the flux if needed
            if (!_Message.IsReading())
                _Message.Invert();

            // reset the flux to the start
            //_Message.resetBufPos(); <- this would reset the stream, but we are using the stream instead of the buffer so this is not good

            return _Message;
        }

        internal void set(BitMemoryStream message)
        {
            _Message = message;

            if (!_Message.IsReading())
                _Message.Invert();
        }

        public override int size()
        {
            // If you change this size, please update IMPULSE_ACTION_HEADER_SIZE in the front-end

            // in bits!!! (the message size and after the message itself)
            return (4 + (_Message?.Length ?? 0)) * 8;
        }

        public override void pack(BitMemoryStream message)
        {
            //byte[] obj = _Message.Buffer();
            //message.serial(ref obj);
            message.SerialBufferWithSize(_Message.Buffer(), _Message.Buffer().Length);
            //throw new System.NotImplementedException();
        }
    }
}