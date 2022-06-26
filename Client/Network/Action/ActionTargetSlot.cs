///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

using Client.Stream;

namespace Client.Network.Action
{
    public class ActionTargetSlot : ActionBase
    {
        /// <summary>
        /// Type of targetting ( LHSTATE::TLHState, only 2 bits are transmitted)
        /// </summary>
        public uint TargetOrPickup;

        /// <summary>
        /// This function creates initializes its fields using the buffer.
        /// </summary>
        public override void Unpack(BitMemoryStream message)
        {
            byte slot = 0;
            message.Serial(ref slot);
            message.Serial(ref TargetOrPickup, 2);
            Slot = slot;
        }

        /// <summary>
        /// Returns the size of this action when it will be send to the UDP connection:
        /// the size is IN BITS, not in bytes (the actual size is this one plus the header size)
        /// </summary>
        public override int Size()
        {
            return /*sizeof(byte)*/ 8 * 8 + 2;
        }

        public override void Reset()
        {
            //throw new NotImplementedException();
        }

        /// <summary>
        /// This function transform the internal field and transform them into a buffer for the UDP connection.
        /// </summary>
        public override void Pack(BitMemoryStream message)
        {
            var slot = Slot;
            message.Serial(ref slot);
            message.Serial(ref TargetOrPickup, 2);
        }
    }
}