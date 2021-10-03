///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

namespace RCC.Network.Action
{
    /// <summary>
    /// Factory for actions - unpacking actions from streams and registering them
    /// </summary>
    public static class ActionFactory
    {
        private const byte InvalidSlot = 0xFF;

        public static Dictionary<ActionCode, KeyValuePair<Type, ActionBase>> RegisteredAction =
            new Dictionary<ActionCode, KeyValuePair<Type, ActionBase>>();

        /// <summary>
        /// upacks an action from a stream - using the right action type
        /// </summary>
        public static ActionBase Unpack(BitMemoryStream message)
        {
            ActionBase action = null;

            if (message.Length * 8 - message.GetPosInBit() >= 8)
            {
                ActionCode code;

                var shortcode = false;
                message.Serial(ref shortcode);

                if (shortcode)
                {
                    uint val = 0;
                    message.Serial(ref val, 2);
                    code = (ActionCode)val;
                }
                else
                {
                    byte val = 0;
                    message.Serial(ref val);
                    code = (ActionCode)val;
                }

                action = Create(InvalidSlot, code);

                if (action == null)
                {
                    RyzomClient.GetInstance().GetLogger().Warn($"Unpacking an action with unknown code, skip it ({code})");
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
        internal static ActionBase Create(byte slot, ActionCode code)
        {
            if (!RegisteredAction.ContainsKey(code))
            {
                RyzomClient.GetInstance().GetLogger().Warn($"CActionFactory::create() try to create an unknown action ({code})");
                return null;
            }

            //if (RegisteredAction[code].Value == null)
            //{
            // no action left in the store
            var action =
                (ActionBase)Activator.CreateInstance(RegisteredAction[code].Key); // execute the factory function

            if (action == null) return null;

            action.Code = code;
            // default, set the property code to the action code (see create(TProperty,TPropIndex))
            action.PropertyCode = code;
            action.Slot = slot;
            action.Reset();
            return action;
            //}
            //else
            //{
            //    // pop an action off the store
            //    var action = RegisteredAction[code].Value;
            //    action.Reset();
            //    action.Slot = slot;
            //    action.PropertyCode = code;
            //    return action;
            //}
        }

        /// <summary>
        /// removes an action from the registered actions
        /// </summary>
        public static void Remove(ActionBase action)
        {
            if (action != null)
            {
                RegisteredAction[action.Code] = new KeyValuePair<Type, ActionBase>(RegisteredAction[action.Code].Key, null);
            }
        }

        /// <summary>
        /// Return the size IN BITS, not in bytes
        /// If you change this size, please update IMPULSE_ACTION_HEADER_SIZE in the front-end
        /// </summary>
        public static int Size(ActionBase action)
        {
            // Warning: when calculating bit sizes, don't forget to multiply sizeof by 8
            int headerBitSize;

            // size of the code
            if ((byte)action.Code < 4)
                // short code (0 1 2 3)
                headerBitSize = 1 + 2; // 3 bit
            else
            {
                headerBitSize = 1 + sizeof(ActionCode); // 9 bit
            }

            return headerBitSize + action.Size();
        }

        /// <summary>
        /// adds an action to the registered actions
        /// </summary>
        internal static void RegisterAction(ActionCode code, Type creator)
        {
            if (!typeof(ActionBase).IsAssignableFrom(creator))
            {
                RyzomClient.GetInstance().GetLogger().Warn($"ActionBase is not assignable from creator {creator}");
                return;
            }

            if ((int)code >= 256)
            {
                RyzomClient.GetInstance().GetLogger().Warn($"Cannot register action code {code} because it exceeds 255");
                return;
            }

            if (RegisteredAction.ContainsKey(code))
            {
                RyzomClient.GetInstance().GetLogger().Warn($"The code {code} already registered in the CActionFactory");
            }
            else
            {
                RegisteredAction.Add(code, new KeyValuePair<Type, ActionBase>(creator, null));
            }
        }

        /// <summary>
        /// Pack an action to a bit stream. Set transmitTimestamp=true for server-->client,
        /// false for client-->server. If true, set the current gamecycle.
        /// </summary>
        public static void Pack(ActionBase action, BitMemoryStream message)
        {
            if ((int)action.Code < 4)
            {
                // short code (0 1 2 3)
                bool shortcode = true;
                uint code = (uint)action.Code;
                message.Serial(ref shortcode);
                message.Serial(ref code, 2);
            }
            else
            {
                bool shortcode = false;
                byte code = (byte)action.Code;
                message.Serial(ref shortcode);
                message.Serial(ref code);
            }

            action.Pack(message);
        }

        const byte PROPERTY_POSITION = 0;

        // <summary>
        // Create the action from a property code, fills property index and fill the internal propindex if needed
        // (it assumes the frontend and the client have the same mapping property/propindex).
        // </summary>
        internal static ActionBase CreateByPropIndex(byte slot, byte propIndex)
        {
            ActionBase action;

            switch (propIndex)
            {
                case PROPERTY_POSITION: // same as propertyId
                    {
                        action = Create(slot, ActionCode.ActionPositionCode);
                        break;
                    }
                default:
                    {
                        action = Create(slot, ActionCode.ActionSint64);
                        ((ActionSint64)action).SetNbBits(propIndex);
                        break;
                    }
            }
            action.PropertyCode = (ActionCode)propIndex;
            return action;
        }
    }
}