///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using Client.Network;
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
        string FxFilename;
        string FxSafeFilename;
        public byte Knowledge;
        List<string> Icons;

        /// <summary>
        /// Constructor
        /// </summary>
        public ForageSourceSheet() : base()
        {

        }

        /// <summary>
        /// Serialize character sheet into binary data file.
        /// </summary>
        /// <param name="f"></param>
        public override void Serial(BitMemoryStream f)
        {
            f.Serial(ref FxFilename);
            f.Serial(ref FxSafeFilename);
            f.Serial(ref Knowledge);

            // workaround
            Icons = new List<string>();
            uint len = 0;
            f.Serial(ref len);

            for (var i = 0; i < len; i++)
            {
                string value = "";
                f.Serial(ref value);

                Icons.Add(value);
            }
        }

        /// <summary>
        /// Serialize character sheet into binary data file.
        /// </summary>
        /// <param name="s"></param>
        public override void Serial(BitStreamFile s)
        {
            s.Serial(out FxFilename);
            s.Serial(out FxSafeFilename);
            s.Serial(out Knowledge);
            s.SerialCont(out Icons);
        }
    }
}