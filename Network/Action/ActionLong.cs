///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

using System.Diagnostics;
using RCC.Property;

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
        private const uint MaxPropertiesPerEntity = Constants.NbVisualProperties;

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
        /// This function creates initializes its fields using the buffer.
        /// </summary>
		/// <param name="message">pointer to the buffer where the data are</param>  
        public override void Unpack(BitMemoryStream message)
        {
            message.Serial(ref _value, _nbBits);
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

        private const int UserDefinedPropertyNbBits = 32;
        private const int PvpModeNbBits = 10;

        // <summary>
        // Register all of properties to set the number of bits
        // that must be transmitted.
        // </summary>
        public static void RegisterNumericPropertiesRyzom()
        {
            RegisterNumericProperty(PropertyType.Orientation, 32); // overridden in fillOutBox
            RegisterNumericProperty(PropertyType.Sheet, 52); // 32(sheet) + 20(row)
            RegisterNumericProperty(PropertyType.Behaviour, 48);
            RegisterNumericProperty(PropertyType.NameStringID, 32); // please do not lower it (or tell Olivier, used for forage sources)
            RegisterNumericProperty(PropertyType.TargetID, 8); // slot
            RegisterNumericProperty(PropertyType.Mode, 44);
            RegisterNumericProperty(PropertyType.Vpa, 64);
            RegisterNumericProperty(PropertyType.Vpb, 47);
            RegisterNumericProperty(PropertyType.Vpc, 58);
            RegisterNumericProperty(PropertyType.EntityMountedID, 8); // slot
            RegisterNumericProperty(PropertyType.RiderEntityID, 8); // slot
            RegisterNumericProperty(PropertyType.Contextual, 16);
            RegisterNumericProperty(PropertyType.Bars, 32); // please do not lower it (or tell Olivier, used for forage sources)
            RegisterNumericProperty(PropertyType.TargetList, UserDefinedPropertyNbBits);
            RegisterNumericProperty(PropertyType.VisualFx, 11); // please do not lower it (or tell Olivier, used for forage sources)
            RegisterNumericProperty(PropertyType.GuildSymbol, 60);
            RegisterNumericProperty(PropertyType.GuildNameID, 32);
            RegisterNumericProperty(PropertyType.EventFactionID, 32);
            RegisterNumericProperty(PropertyType.PvpMode, PvpModeNbBits);
            RegisterNumericProperty(PropertyType.PvpClan, 32);
            RegisterNumericProperty(PropertyType.OwnerPeople, 3); // 4 races and unknow
            RegisterNumericProperty(PropertyType.OutpostInfos, 16); // 15+1
        }

        // <summary>
        // Register a property to set the number of bits
        // that must be transmitted.
        // </summary>
        public static void RegisterNumericProperty(PropertyType propIndex, int nbbits)
        {
            Debug.Assert(nbbits > 0 && nbbits <= 64);
            PropertyToNbBit[(int)propIndex] = nbbits;
        }
    }
}