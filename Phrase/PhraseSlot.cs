///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

using RCC.Network;

namespace RCC.Phrase
{
    /// <summary>Tuple Sabrina / Known Slot.</summary>
    /// <author>Lionel Berenguier</author>
    /// <author>Nevrax France</author>
    /// <date>2003</date>
    public class PhraseSlot
    {
        internal PhraseCom Phrase;
        internal ushort KnownSlot;
        internal uint PhraseSheetId;

        public static PhraseSlot Serial(BitMemoryStream impulse)
        {
            var ret = new PhraseSlot { Phrase = PhraseCom.Serial(impulse) };

            impulse.Serial(ref ret.KnownSlot);
            impulse.Serial(ref ret.PhraseSheetId);

            return ret;
        }
    }
}
