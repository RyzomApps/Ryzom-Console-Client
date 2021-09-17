// This code is a modified version of a file from the 'Ryzom - MMORPG Framework'
// <http://dev.ryzom.com/projects/ryzom/>,
// which is released under GNU Affero General Public License.
// <http://www.gnu.org/licenses/>
// Original Copyright 2010 by Winch Gate Property Limited

using System;
using System.Collections.Generic;
using System.Reflection;
using RCC.Helper;
using RCC.Network;

namespace RCC.NetworkAction
{
    /// <summary>
    /// Factory for actions - unpacking actions from streams and registering them
    /// </summary>
    public static class ActionFactory
    {
        const byte InvalidSlot = 0xFF;

        public static Dictionary<ActionCode, KeyValuePair<Type, Action>> RegisteredAction =
            new Dictionary<ActionCode, KeyValuePair<Type, Action>>();

        /// <summary>
        /// upacks an action from a stream - using the right action type
        /// </summary>
        public static Action Unpack(BitMemoryStream message, bool b)
        {
            Action action = null;

            // 32 1 32 1 1 1 2

            if (message.Length * 8 - message.GetPosInBit() >= 8)
            {
                ActionCode code;

                bool shortcode = false;
                message.Serial(ref shortcode);

                if (shortcode)
                {
                    uint val = 0;
                    message.Serial(ref val, 2);
                    code = (ActionCode) val;
                }
                else
                {
                    byte val = 0;
                    message.Serial(ref val);
                    code = (ActionCode) val;
                }

                action = Create(InvalidSlot, code);

                if (action == null)
                {
                    RyzomClient.Log?.Warn($"Unpacking an action with unknown code, skip it ({code})");
                }
                else
                {
                    action.Unpack(message);
                }
            }

            return action;
        }

        /// <summary>
        /// creates instances of an action based on the given action code
        /// </summary>
        internal static Action Create(byte slot, ActionCode code)
        {
            if (!RegisteredAction.ContainsKey(code))
            {
                RyzomClient.Log?.Warn($"CActionFactory::create() try to create an unknown action ({code})");
                return null;
            }

            if (RegisteredAction[code].Value == null)
            {
                // no action left in the store
                var action =
                    (Action) Activator.CreateInstance(RegisteredAction[code].Key); // execute the factory function
                //nlinfo( "No action in store for code %u, creating action (total %u, total for code %u)", code, getNbActionsInStore(), getNbActionsInStore(action.Code) );

                if (action == null) return null;

                action.Code = code;
                action.PropertyCode =
                    code; // default, set the property code to the action code (see create(TProperty,TPropIndex))
                action.Slot = slot;
                action.Reset();
                return action;
            }
            else
            {
                // pop an action off the store
                var action = RegisteredAction[code].Value;
                //nlinfo( "Found action in store for code %u (total %u, total for code %u)", code, getNbActionsInStore(), getNbActionsInStore(action.Code) );
                //RegisteredAction[code].Value.pop_back();
                action.Reset();
                action.Slot = slot;
                action.PropertyCode = code;
                return action;
            }
        }

        /// <summary>
        /// removes an action from the registered actions
        /// </summary>
        public static void Remove(Action action)
        {
            if (action != null)
            {
                RegisteredAction[action.Code] = new KeyValuePair<Type, Action>(RegisteredAction[action.Code].Key, null);
                //nlinfo( "Inserting action in store for code %u (total %u, total for code %u)", action.Code, getNbActionsInStore(), getNbActionsInStore(action.Code) );
            }
        }

        /// <summary>
        ///     Return the size IN BITS, not in bytes
        /// </summary>
        public static int Size(Action action)
        {
            // If you change this size, please update IMPULSE_ACTION_HEADER_SIZE in the front-end

            /*
             * Warning: when calculating bit sizes, don't forget to multiply sizeof by 8
             */
            int headerBitSize;

            // size of the code

            if (action.Code < (ActionCode) 4)
                headerBitSize = 1 + 2;
            else
            {
                RyzomClient.Log?.Warn($"{MethodBase.GetCurrentMethod()?.Name} called, but not implemented");
                // TODO: fix that (sizeof(()action.Code) * 8) <- bdh: whats that about?
                headerBitSize = 1 + /*(sizeof(()action.Code) * 8)*/ 8;
            }

            return headerBitSize + action.Size();
        }

        /// <summary>
        /// adds an action to the registered actions
        /// </summary>
        internal static void RegisterAction(ActionCode code, Type creator)
        {
            if (!typeof(Action).IsAssignableFrom(creator))
            {
                RyzomClient.Log?.Warn($"Action is not assignable from creator {creator}");
                return;
            }

            if ((int) code >= 256)
            {
                RyzomClient.Log?.Warn($"Cannot register action code {code} because it exceeds 255");
                return;
            }

            if (RegisteredAction.ContainsKey(code))
            {
                RyzomClient.Log?.Warn($"The code {code} already registered in the CActionFactory");
            }
            else
            {
                RegisteredAction.Add(code, new KeyValuePair<Type, Action>(creator, null));
            }
        }

        /// <summary>
        ///     Pack an action to a bit stream. Set transmitTimestamp=true for server-->client,
        ///     false for client-->server. If true, set the current gamecycle.
        /// </summary>
        public static void Pack(Action action, BitMemoryStream message)
        {
            //H_BEFORE(FactoryPack);
            //sint32 val = message.getPosInBit ();

            // TODO: evaluate this

            if ((int) action.Code < 4)
            {
                // short code (0 1 2 3)
                bool shortcode = true;
                uint code = (uint) action.Code;
                message.Serial(ref shortcode);
                message.SerialAndLog2(ref code, 2);
            }
            else
            {
                bool shortcode = false;
                short code = (short) action.Code;
                message.Serial(ref shortcode);
                message.Serial(ref code);
            }

            action.Pack(message);
            //H_AFTER(FactoryPack);

            //OLIV: nlassertex (message.getPosInBit () - val == (sint32)CActionFactory::getInstance()->size (action), ("CActionFactory::pack () : action %d packed %u bits, should be %u, size() is wrong", action->Code, message.getPosInBit () - val, CActionFactory::getInstance()->size (action)));

            //	nlinfo ("ac:%p pack one action in message %d %hu %u %d", action, action->Code, (uint16)(action->CLEntityId), val, message.getPosInBit()-val);
        }
    }
}