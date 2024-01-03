///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

using System;

namespace Client.Sheet
{
    /// <summary>
    /// Use 8 bits file type and 24 bits id 
    /// </summary>
    internal class IdInfos
    {
        // 0 Type0
        // 1 Id0
        // 2 Id1
        // 3 Id2
        private byte[] _bytes = new byte[4];

        /// <summary>
        /// 4 byte type and id
        /// </summary>
        public uint Id
        {
            get => BitConverter.ToUInt32(_bytes);
            set => _bytes = BitConverter.GetBytes(value);
        }

        /// <summary>
        /// 1 byte type
        /// </summary>
        public uint Type
        {
            get => _bytes[0];
            set => _bytes[0] = BitConverter.GetBytes(value)[0];
        }

        /// <summary>
        /// 3 byte short id
        /// </summary>
        public uint ShortId
        {
            get
            {
                // I2 I1 I0 X -> 0 I2 I1 I0
                return BitConverter.ToUInt32(new byte[] { _bytes[1], _bytes[2], _bytes[3], 0 });
            }
            set
            {
                // X I2 I1 I0 -> I2 I1 I0 X
                var tmp = BitConverter.GetBytes(value);
                _bytes[1] = tmp[0];
                _bytes[2] = tmp[1];
                _bytes[3] = tmp[2];
            }
        }
    }
}
