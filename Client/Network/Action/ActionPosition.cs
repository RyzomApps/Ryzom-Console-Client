///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

using System;

namespace Client.Network.Action
{
    internal class ActionPosition : ActionBase
    {
        /// <summary>
        /// Returns true if the property is continuous
        /// </summary>
        public override bool IsContinuous() { return true; }

        public int[] Position = new int[3];
        public ushort[] Position16 = new ushort[3];

        public bool IsRelative;
        public bool Interior;

        public uint GameCycle { get; internal set; }

        public override int Size() { return 3 * 16; }

        /// <summary>
        /// Unpacks the positions from the bitmemstream into Position16[]
        /// (Position[] must then be filled externally)
        /// </summary>
        public override void Unpack(BitMemoryStream message)
        {
            message.Serial(ref Position16[0]);
            message.Serial(ref Position16[1]);
            message.Serial(ref Position16[2]);
            IsRelative = (Position16[2] & 0x1) != 0;
            Interior = (Position16[2] & 0x2) != 0;
        }

        public override void Pack(BitMemoryStream message)
        {
            throw new NotImplementedException();
        }

        public override void Reset()
        {
            // TODO: implement ActionPosition Reset
            //throw new NotImplementedException();
        }
    }
}