///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

using Client.Network;
using Client.Sheet;

namespace Client.Phrase
{
    /// <summary>Tuple Sabrina / Known Slot.</summary>
    /// <author>Lionel Berenguier</author>
    /// <author>Nevrax France</author>
    /// <date>2003</date>
    public class PhraseSlot
    {
        internal PhraseCom Phrase;
        internal ushort KnownSlot;
        internal SheetId PhraseSheetId;

        public static PhraseSlot Serial(BitMemoryStream impulse)
        {
            var ret = new PhraseSlot { Phrase = PhraseCom.Serial(impulse) };

            impulse.Serial(ref ret.KnownSlot);

            uint sheetid = 0;
            impulse.Serial(ref sheetid);

            ret.PhraseSheetId = new SheetId(sheetid);

            return ret;
        }
    }
}
