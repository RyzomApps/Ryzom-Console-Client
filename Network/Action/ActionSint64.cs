///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

using RCC.Network.Action;

namespace RCC.Network
{
    /// <summary>
	/// Action containing a SInt64 value.
	/// To create such an object, call CActionFactory::create( TProperty, TPropIndex ).
	/// </summary>
	/// <author>Olivier Cado</author>
	/// <author>Nevrax France</author>
	/// <date>2002</date>
    public class ActionSint64 : ActionBase
    {
        // Number of visual properties
        const uint NB_VISUAL_PROPERTIES = 28;

        const uint MAX_PROPERTIES_PER_ENTITY = NB_VISUAL_PROPERTIES;

        public static ActionBase Create()
        {
            return new ActionSint64();
        }

        /// Register a property to set the number of bits
		/// that must be transmitted.
        static void RegisterNumericProperty(byte propIndex, uint nbbits) { }

        /// TEMP
        static void RegisterNumericPropertiesRyzom() { }

        /// This function creates initializes its fields using the buffer.
		/// \param buffer pointer to the buffer where the data are
		/// \size size of the buffer
		///
        public override void Unpack(BitMemoryStream message)
        {
            RyzomClient.GetInstance().GetLogger().Warn("ActionSint64.Unpack() is not yet implemented!");
        }

        /// This functions is used when you want to transform an action into an IStream.
        void Serial(BitMemoryStream f) { }

        /// Returns the size of this action when it will be send to the UDP connection:
		/// the size is IN BITS, not in bytes (the actual size is this one plus the header size)
		///
        public override int Size()
        {
            return (int)_NbBits;
        }

        /// Returns the maximum size of this action (INCLUDING the header size handled by CActionFactory!)
        public static uint GetMaxSizeInBit()
        {
            return 64;
        }

        /// Sets the value of the action
        public virtual void SetValue(long value)
        {
            _Value = (ulong)value;
        }

        /// Same, but avoids virtual
        public void SetValue64(long value)
        {
            _Value = (ulong)value;
        }

        /// Sets the number of bits to transmit
        public void SetNbBits(byte propIndex)
        {
            _NbBits = _PropertyToNbBit[propIndex];
        }

        /// Gets the value of the action
        public long GetValue()
        {
            return (long)_Value;
        }

        /// Returns false because the action is not a "continuous action".
		/// BUT the property may be continuous, without using the benefits of CContinuousAction (deltas).
		///
        public override bool IsContinuous()
        {
            return false;
        }


        public void SetAndPackValue(long value, BitMemoryStream outMsg)
        {
            _Value = (ulong)value;
            outMsg.Serial(ref _Value, (int)_NbBits);
        }

        public void SetAndPackValueNBits(long value, BitMemoryStream outMsg, uint nbits)
        {
            _Value = (ulong)value;
            outMsg.Serial(ref _Value, (int)_NbBits);
        }

        public void packFast(BitMemoryStream outMsg)
        {
            outMsg.Serial(ref _Value, (int)_NbBits);
        }


        private ulong _Value;
        private uint _NbBits = new uint();

        /// Constructor
        private ActionSint64()
        {
            this._Value = (long)0;
            this._NbBits = 0;
        }

        /// This function transform the internal field and transform them into a buffer for the UDP connection.
        /// \param buffer pointer to the buffer where the data will be written
        /// \size size of the buffer
        ///
        public override void Pack(BitMemoryStream message) { }

        /// This method intialises the action with a default state///
        public override void Reset()
        {
            _Value = 0;
            _NbBits = 0;
        }

        /// Init
        private static uint[] _PropertyToNbBit = new uint[MAX_PROPERTIES_PER_ENTITY];
    }

}