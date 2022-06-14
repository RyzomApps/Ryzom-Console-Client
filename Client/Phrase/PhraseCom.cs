///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using API.Helper;
using Client.Network;
using Client.Sheet;

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
                //_serialSbrickType = CSheetId.typeFromFileExtension("sbrick");
            }

            // read
            //impulse.SerialCont(ref _serialCompBricks);
            int len = 0;
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

            ret.Bricks.Resize(serialCompBricks.Count);

            for (var i = 0; i < ret.Bricks.Count; i++)
            {
                if (serialCompBricks[i] == 0)
                {
                    ret.Bricks[i] = null;
                }
                else
                {
                    // TODO Bricks[i].BuildSheetId(_serialCompBricks[i] - 1, _serialSbrickType);
                }
            }

            return ret;
        }
    }
}
