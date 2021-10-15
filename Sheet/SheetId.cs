///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

namespace RCC.Sheet
{
    public class SheetId
    {
        public static SheetId Unknown = new SheetId(0);

        private uint _id;

        public SheetId()
        {

        }

        public SheetId(uint sheetRef)
        {
            _id = sheetRef;
        }
    }
}