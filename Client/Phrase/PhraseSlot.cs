///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

using Client.Network;
using Client.Sheet;
using Client.Stream;

namespace Client.Phrase
{
    /// <summary>Tuple Sabrina / Known Slot</summary>
    /// <author>Lionel Berenguier</author>
    /// <author>Nevrax France</author>
    /// <date>2003</date>
    public class PhraseSlot
    {
        internal PhraseCom Phrase;
        internal ushort KnownSlot;
        internal SheetId PhraseSheetId;

        /// <summary>
        /// This serial is made for server->client com
        /// </summary>
        public static PhraseSlot Serial(BitMemoryStream impulse, SheetIdFactory sheetIdFactory)
        {
            var ret = new PhraseSlot { Phrase = PhraseCom.Serial(impulse, sheetIdFactory) };
            impulse.Serial(ref ret.KnownSlot);

            uint sheetid = 0;
            impulse.Serial(ref sheetid);

            ret.PhraseSheetId = sheetIdFactory.SheetId(sheetid);

            return ret;
        }
    }
}
