///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using Client.Sheet;
using Client.Stream;

namespace Client.Forage
{
    /// <summary>
    /// Class to manage the forage source sheet.
    /// </summary>
    /// <author>Olivier Cado</author>
    /// <author>Nevrax France</author>
    /// <date>2003</date>
    public class ForageSourceSheet : EntitySheet
    {
        private string _fxFilename;
        private string _fxSafeFilename;
        private List<string> _icons;

        public byte Knowledge;

        /// <summary>
        /// Serialize character sheet into binary data file.
        /// </summary>
        public override void Serial(BitMemoryStream f)
        {
            f.Serial(ref _fxFilename);
            f.Serial(ref _fxSafeFilename);
            f.Serial(ref Knowledge);

            // workaround
            _icons = new List<string>();

            uint len = 0;
            f.Serial(ref len);

            for (var i = 0; i < len; i++)
            {
                var value = "";
                f.Serial(ref value);

                _icons.Add(value);
            }
        }

        /// <summary>
        /// Serialize character sheet into binary data file.
        /// </summary>
        public override void Serial(BitStreamFile s)
        {
            s.Serial(out _fxFilename);
            s.Serial(out _fxSafeFilename);
            s.Serial(out Knowledge);
            s.SerialCont(out _icons);
        }

        public ForageSourceSheet(SheetIdFactory sheetIdFactory) : base(sheetIdFactory) { }
    }
}