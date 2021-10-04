///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

using System;
using RCC.Entity;

namespace RCC.Network.Action
{
    /// <summary>
	/// Action containing a SInt64 value.
	/// To create such an object, call CActionFactory::create( TProperty, TPropIndex ).
	/// </summary>
	/// <author>Olivier Cado</author>
	/// <author>Nevrax France</author>
	/// <date>2002</date>
    public class ActionLong : ActionBase
    {
        /// <summary>Number of visual properties</summary>
        private const uint MaxPropertiesPerEntity = VpNodeBase.NbVisualProperties;

        private ulong _value;
        private int _nbBits;

        /// Init
        private static readonly int[] PropertyToNbBit = new int[MaxPropertiesPerEntity];

        /// <summary>
        /// Constructor
        /// </summary>
        public ActionLong()
        {
            _value = 0;
            _nbBits = 0;
        }

        public static ActionBase Create()
        {
            return new ActionLong();
        }

        /// <summary>
        /// Register a property to set the number of bits
		/// that must be transmitted.
		/// </summary>
        public static void RegisterNumericProperty(byte propIndex, uint nbbits)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// This function creates initializes its fields using the buffer.
        /// </summary>
		/// <param name="message">pointer to the buffer where the data are</param>  
        public override void Unpack(BitMemoryStream message)
        {
            message.Serial(ref _value, _nbBits);
        }

        /// <summary>
        /// This functions is used when you want to transform an action into an IStream.
        /// </summary>
        private void Serial(BitMemoryStream f)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns the size of this action when it will be send to the UDP connection:
		/// the size is IN BITS, not in bytes (the actual size is this one plus the header size)
		/// </summary>
        public override int Size()
        {
            return _nbBits;
        }

        /// <summary>
        /// Returns the maximum size of this action (INCLUDING the header size handled by CActionFactory!)
        /// </summary>
        public static uint GetMaxSizeInBit()
        {
            return 64;
        }

        /// <summary>
        /// Sets the value of the action
        /// </summary>
        public virtual void SetValue(long value)
        {
            _value = (ulong)value;
        }

        /// <summary>
        /// Sets the value of the action, but avoids virtual
        /// </summary>
        public void SetValue64(long value)
        {
            _value = (ulong)value;
        }

        /// <summary>
        /// Sets the number of bits to transmit
        /// </summary>
        public void SetNbBits(byte propIndex)
        {
            _nbBits = PropertyToNbBit[propIndex];
        }

        /// <summary>
        /// Gets the value of the action
        /// </summary>
        public long GetValue()
        {
            return (long)_value;
        }

        /// <summary>
        /// Returns false because the action is not a "continuous action".
		/// BUT the property may be continuous, without using the benefits of CContinuousAction (deltas).
		/// </summary>
        public override bool IsContinuous()
        {
            return false;
        }


        public void SetAndPackValue(long value, BitMemoryStream outMsg)
        {
            _value = (ulong)value;
            outMsg.Serial(ref _value, (int)_nbBits);
        }

        public void SetAndPackValueNBits(long value, BitMemoryStream outMsg, uint nbits)
        {
            _value = (ulong)value;
            outMsg.Serial(ref _value, (int)_nbBits);
        }

        public void PackFast(BitMemoryStream outMsg)
        {
            outMsg.Serial(ref _value, (int)_nbBits);
        }


        /// <summary>
        /// This function transform the internal field and transform them into a buffer for the UDP connection.
        /// </summary>
        /// <param name="message">buffer pointer to the buffer where the data will be written</param>
        public override void Pack(BitMemoryStream message) { }

        /// <summary>
        /// This method intialises the action with a default state
        /// </summary>
        public override void Reset()
        {
            _value = 0;
            _nbBits = 0;
        }
    }
}