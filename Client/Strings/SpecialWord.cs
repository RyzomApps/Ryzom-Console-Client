///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

using System.Diagnostics;
using Client.Stream;

namespace Client.Strings
{
    /// <summary>
    /// SpecialItems
    /// </summary>
    public class SpecialWord
    {
        /// <summary>
        /// The Name of the item
        /// </summary>
        public string Name = "";

        /// <summary>
        /// The Women Name of the item
        /// </summary>
        public string WomenName = "";

        /// <summary>
        /// Description of the item
        /// </summary>
        public string Desc = "";

        /// <summary>
        /// Optional Second description (For SBrick composition for example)
        /// </summary>
        public string Desc2 = "";

        public void Serial(BitMemoryStream f)
        {
            var ver = f.SerialVersion(2);

            if (ver >= 2)
            {
                f.Serial(ref Name);
                f.Serial(ref WomenName);
                f.Serial(ref Desc);
                f.Serial(ref Desc2);
            }
            else
            {
                Debug.Assert(f.IsReading());

                var name = ""; // Old UTF-16 serial
                var womenName = ""; // Old UTF-16 serial
                var desc = ""; // Old UTF-16 serial
                var desc2 = ""; // Old UTF-16 serial

                f.Serial(ref name);

                if (ver >= 1)
                {
                    f.Serial(ref womenName);
                }

                f.Serial(ref desc);
                f.Serial(ref desc2);

                Name = name;
                WomenName = womenName;
                Desc = desc;
                Desc2 = desc2;
            }
        }
    }

}
