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

namespace Client.Phrase
{
    public class PhraseCom
    {
        private readonly uint _serialSbrickType = 0;

        //private readonly List<ushort> _serialCompBricks = new List<ushort>();

        public string Name;

        public List<SheetId> Bricks = new List<SheetId>();

        public static PhraseCom EmptyPhrase = new PhraseCom();

        public static PhraseCom Serial(BitMemoryStream impulse)
        {
            var ret = new PhraseCom();

            if (!impulse.IsReading())
            {
                throw new NotImplementedException();
            }

            impulse.Serial(ref ret.Name, false);

            // Get the type of .sbrick
            if (ret._serialSbrickType == 0)
            {
                //_serialSbrickType = SheetId.TypeFromFileExtension("sbrick");
            }

            // read
            // workaround for impulse.SerialCont(ref _serialCompBricks);

            var len = 0;
            impulse.Serial(ref len);

            // 16 bits compression of the Bricks
            var serialCompBricks = new List<ushort>(len);

            for (var i = 0; i < len; i++)
            {
                ushort value = 0;
                impulse.Serial(ref value);
                serialCompBricks.Add(value);
            }

            // uncompress
            //ContReset(Bricks);

            // workaround for: ret.Bricks.Resize(serialCompBricks.Count);

            ret.Bricks.Clear();

            for (var i = 0; i < serialCompBricks.Count; i++)
            {
                ret.Bricks.Add(new SheetId());
            }

            // end workaround

            for (var i = 0; i < ret.Bricks.Count; i++)
            {
                if (serialCompBricks[i] == 0)
                {
                    ret.Bricks[i] = null;
                }
                else
                {
                    ret.Bricks[i].BuildSheetId(serialCompBricks[i] - 1, EntitySheet.SheetType.SBRICK);
                }
            }

            return ret;
        }
    }
}
