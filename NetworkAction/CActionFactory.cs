﻿using System;
using System.Collections.Generic;
using System.Reflection;
using RCC.Helper;
using RCC.Network;

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

            if (message.Length * 8 - message.getPosInBit() >= 8)
            {
                TActionCode code;

                bool shortcode = false;
                message.serial(ref shortcode);

                if (shortcode)
                {
                    uint val = 0;
                    message.serial(ref val, 2);
                    code = (TActionCode)val;
                }
                else
                {
                    byte val = 0;
                    message.serial(ref val);
                    code = (TActionCode)val;
                }

                action = create(INVALID_SLOT, code);

                if (action == null)
                {
                    ConsoleIO.WriteLineFormatted($"§cUnpacking an action with unknown code, skip it ({code})");
                }
                else
                {
                    action.unpack(message);
                }
            }

            return action;
        }

        internal static CAction create(byte slot, TActionCode code)
        {
            if (!RegisteredAction.ContainsKey(code))
            {
                ConsoleIO.WriteLine("CActionFactory::create() try to create an unknown action (" + code + ")");
                return null;
            }
            else if (RegisteredAction[code].Value == null)
            {
                // no action left in the store
                var action = (CAction)Activator.CreateInstance(RegisteredAction[code].Key); // execute the factory function
                                                                                            //nlinfo( "No action in store for code %u, creating action (total %u, total for code %u)", code, getNbActionsInStore(), getNbActionsInStore(action.Code) );

                if (action == null) return null;

                action.Code = code;
                action.PropertyCode =
                    code; // default, set the property code to the action code (see create(TProperty,TPropIndex))
                action.Slot = slot;
                action.reset();
                return action;

            }
            else
            {
                // pop an action off the store
                var action = RegisteredAction[code].Value;
                //nlinfo( "Found action in store for code %u (total %u, total for code %u)", code, getNbActionsInStore(), getNbActionsInStore(action.Code) );
                //RegisteredAction[code].Value.pop_back();
                action.reset();
                action.Slot = slot;
                action.PropertyCode = code;
                return action;
            }
        }

        public static void remove(CAction action)
        {
            if (action != null)
            {
                RegisteredAction[action.Code] = new KeyValuePair<Type, CAction>(RegisteredAction[action.Code].Key, null);
                //nlinfo( "Inserting action in store for code %u (total %u, total for code %u)", action.Code, getNbActionsInStore(), getNbActionsInStore(action.Code) );
            }
        }

        //public static void remove(CActionImpulsion action)
        //{
        //    if (action != NULL)
        //    {
        //        CAction* ptr = static_cast<CAction*>(action);
        //        remove(ptr);
        //        action = NULL;
        //    }
        //}

        /// <summary>
        /// Return the size IN BITS, not in bytes
        /// </summary>
        public static int size(CAction action)
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
                ConsoleIO.WriteLineFormatted("§c" + MethodBase.GetCurrentMethod().Name + " called, but not implemented");
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

        /// <summary>
        /// Pack an action to a bit stream. Set transmitTimestamp=true for server-->client,
        /// false for client-->server. If true, set the current gamecycle.
        /// </summary>
        public static void pack(CAction action, CBitMemStream message)
        {
            //H_BEFORE(FactoryPack);
            //sint32 val = message.getPosInBit ();

            // TODO: evaluate this

            if ((int)action.Code < 4)
            {
                // short code (0 1 2 3)
                bool shortcode = true;
                uint code = (uint)action.Code;
                message.serial(ref shortcode);
                message.serialAndLog2(ref code, 2);
            }
            else
            {
                bool shortcode = false;
                short code = (short)action.Code;
                message.serial(ref shortcode);
                message.serial(ref code);
            }

            action.pack(message);
            //H_AFTER(FactoryPack);

            //OLIV: nlassertex (message.getPosInBit () - val == (sint32)CActionFactory::getInstance()->size (action), ("CActionFactory::pack () : action %d packed %u bits, should be %u, size() is wrong", action->Code, message.getPosInBit () - val, CActionFactory::getInstance()->size (action)));

            //	nlinfo ("ac:%p pack one action in message %d %hu %u %d", action, action->Code, (uint16)(action->CLEntityId), val, message.getPosInBit()-val);
        }
    }
}