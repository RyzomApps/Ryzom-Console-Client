using System;

namespace RCC.Network
{
    public static class CActionFactory
    {
        const byte INVALID_SLOT = 0xFF;

        public static CAction unpack(CBitMemStream message, bool b)
        {
            CAction action = null;

            if (message.Length * 8 - message.Pos >= 8)
            {
                TActionCode code;

                bool shortcode = false;
                message.serial(ref shortcode);

                if (shortcode)
                {
                    //code = 0;
                    short val = 0;
                    message.serial(ref val);
                    code = (TActionCode)val;
                }
                else
                {
                    byte codeB = 0;
                    message.serial(ref codeB);
                    code = (TActionCode)codeB;
                }

                action = create(INVALID_SLOT, code);

                if (action == null)
                {
                    ConsoleIO.WriteLine($"Unpacking an action with unknown code, skip it ({code})");
                }
                else
                {
                    action.unpack(message);
                }
            }

            return action;
        }

        private static CAction create(byte invalidSlot, TActionCode code)
        {
            ConsoleIO.WriteLine($"CAction create ({invalidSlot}, {code})");

            return new CAction();

            //throw new NotImplementedException();
        }

        public static void remove(CAction action)
        {
            ConsoleIO.WriteLine($"CAction remove ({action})");

            //throw new NotImplementedException();
        }

        /// <summary>
        /// Return the size IN BITS, not in bytes
        /// </summary>
        public static object size(CAction action)
        {
            // If you change this size, please update IMPULSE_ACTION_HEADER_SIZE in the front-end

            /*
             * Warning: when calculating bit sizes, don't forget to multiply sizeof by 8
             */
            int headerBitSize;

            // size of the code

            if (action.Code < (TActionCode)4)
                headerBitSize = 1 + 2;
            else
                // TODO: fix that (sizeof(()action.Code) * 8) <- bdh: whats that about?
                headerBitSize = 1 + /*(sizeof(()action.Code) * 8)*/ 8;

            return headerBitSize + action.size();
        }
    }
}