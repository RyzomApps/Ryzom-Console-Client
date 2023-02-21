///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

using Client.Stream;
using API.Sheet;

namespace Client.Sheet
{
    /// <summary>
    /// Class to manage an entity sheet
    /// </summary>
    /// <author>Guillaume PUZIN</author>
    /// <author>Nevrax France</author>
    /// <date>2001</date>
    public abstract class EntitySheet : IEntitySheet
    {
        //private readonly SheetIdFactory _sheetIdFactory;

        /// <summary>
        /// Type of the sheet
        /// </summary>
        protected API.Sheet.SheetType _type;

        /// <summary>
        /// Sheet Id
        /// </summary>
        public SheetId Id;

        /// <summary>
        /// Constructor
        /// </summary>
        protected EntitySheet(SheetIdFactory sheetIdFactory)
        {
            //_sheetIdFactory = sheetIdFactory;
            _type = API.Sheet.SheetType.UNKNOWN_SHEET_TYPE;
            Id = new SheetId(sheetIdFactory);
        }

        /// <summary>
        /// Return the type of the sheet
        /// </summary>
        public SheetType Type
        {
            get => _type;
            set => _type = value;
        }

        /// <summary>
        /// Serialize character sheet into binary data file
        /// </summary>
        public abstract void Serial(BitMemoryStream f);

        public abstract void Serial(BitStreamFile s);
    }
}
