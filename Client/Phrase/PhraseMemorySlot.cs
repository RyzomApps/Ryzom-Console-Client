///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

using Client.Network;
using Client.Stream;

namespace Client.Phrase
{
    public class PhraseMemorySlot
    {
        internal byte MemoryLineId;
        internal byte MemorySlotId;
        internal ushort PhraseId;

        /// <summary>
        /// This serial is made for server->client com.
        /// </summary>
        public static PhraseMemorySlot Serial(BitMemoryStream impulse)
        {
            var ret = new PhraseMemorySlot();

            impulse.Serial(ref ret.MemoryLineId);
            impulse.Serial(ref ret.MemorySlotId);
            impulse.Serial(ref ret.PhraseId);

            return ret;
        }
    }
}