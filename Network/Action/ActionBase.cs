///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

namespace RCC.Network.Action
{
    /// <summary>
    ///     Abstract base for other actions
    /// </summary>
    public abstract class ActionBase
    {
        /// <summary>
        ///     code that specifies the type of the action
        /// </summary>
        public ActionCode Code { get; internal set; }

        public ActionCode PropertyCode { get; internal set; }

        public byte Slot { get; internal set; }

        /// <summary>
        ///     unpack a message from stream
        /// </summary>
        public abstract void Unpack(BitMemoryStream message);

        /// <summary>
        ///     Returns the size of this action when it will be send to the UDP connection:
        ///     the size is IN BITS, not in bytes(the actual size is this one plus the header size)
        /// </summary>
        public abstract int Size();

        public abstract void Reset();

        /// <summary>
        ///     pack a message for the stream
        /// </summary>
        public abstract void Pack(BitMemoryStream message);
    }
}