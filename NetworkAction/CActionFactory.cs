using System;
using System.Collections.Generic;
using System.Reflection;

namespace RCC.NetworkAction
{
    public static class CActionFactory
    {
        const byte INVALID_SLOT = 0xFF;

        public static Dictionary<TActionCode, KeyValuePair<Type, CAction>> RegisteredAction = new Dictionary<TActionCode, KeyValuePair<Type, CAction>>();

        public static CAction unpack(CBitMemStream message, bool b)
        {
            CAction action = null;

            // 32 1 32 1 1 1 2

            if (message.Length * 8 - message.Pos >= 8)
            {
                TActionCode code;

                bool shortcode = false;
                message.serial(ref shortcode);

                if (shortcode)
                {
                    //code = 0;
                    short val = 0;
                    message.serial(ref val, 2);
                    code = (TActionCode)val;
                }
                else
                {
                    byte codeB = 0;
                    message.serial(ref codeB);
                    code = (TActionCode)codeB;
                }

                // todo: need the right action here i think ;)
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

        private static CAction create(byte slot, TActionCode code)
        {
            if (!RegisteredAction.ContainsKey(code))
            {
                ConsoleIO.WriteLine("CActionFactory::create() try to create an unknown action (" + code + ")");
                return null;
            }
            else if (RegisteredAction[code].Value == null)
            {
                // no action left in the store
                CAction action = (CAction)Activator.CreateInstance(RegisteredAction[code].Key); // execute the factory function
                //nlinfo( "No action in store for code %u, creating action (total %u, total for code %u)", code, getNbActionsInStore(), getNbActionsInStore(action->Code) );
                action.Code = code;
                action.PropertyCode = code;    // default, set the property code to the action code (see create(TProperty,TPropIndex))
                action.Slot = slot;
                action.reset();
                return action;
            }
            else
            {
                // pop an action off the store
                CAction action = RegisteredAction[code].Value;
                //nlinfo( "Found action in store for code %u (total %u, total for code %u)", code, getNbActionsInStore(), getNbActionsInStore(action->Code) );
                //RegisteredAction[code].Value.pop_back();
                action.reset();
                action.Slot = slot;
                action.PropertyCode = code;
                return action;
            }
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
            {
                ConsoleIO.WriteLine(MethodBase.GetCurrentMethod().Name + " called, but not implemented");
                // TODO: fix that (sizeof(()action.Code) * 8) <- bdh: whats that about?
                headerBitSize = 1 + /*(sizeof(()action.Code) * 8)*/ 8;
            }

            return headerBitSize + action.size();
        }



        internal static void registerAction(TActionCode code, Type creator)
        {
            if (!typeof(CAction).IsAssignableFrom(creator))
            {
                ConsoleIO.WriteLine("CAction is not assignable from creator " + creator);
                return;
            }

            if ((int)code >= 256)
            {
                ConsoleIO.WriteLine("Cannot register action code " + code + " because it exceeds 255");
                return;
            }

            if (RegisteredAction.ContainsKey(code))
            {
                ConsoleIO.WriteLine("The code " + code + " already registered in the CActionFactory");
            }
            else
            {
                RegisteredAction.Add(code, new KeyValuePair<Type, CAction>(creator, null));
            }
        }
    }
}