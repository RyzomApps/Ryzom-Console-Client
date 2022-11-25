///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using Client.Sheet;
using Client.Stream;

namespace Client.Phrase
{
    /// <summary>
    /// Description of a Sabrina Phrase. (I.e. set of brick, and other client side infos)
    /// For communication Client/Server (NB: CSPhrase name already exist...)
    /// <author>Lionel Berenguier</author>
    /// <author>Nevrax France</author>
    /// <date>2003</date>
    /// </summary>
    public class PhraseCom
    {
        private uint _serialSbrickType;

        /// <summary>
        /// Name Of the Phrase. Saved on server, read on client.
        /// </summary>
        public string Name;

        /// <summary>
        /// List Of SBricks composing the phrase.
        /// </summary>
        public List<SheetId> Bricks = new List<SheetId>();

        /// <summary>
        /// Index into Bricks to use as icon (if out of range, then automatic icon selection)
        /// </summary>
        public byte IconIndex = byte.MaxValue;

        /// <summary>
        /// Empty element
        /// </summary>
        public static PhraseCom EmptyPhrase = new PhraseCom();

        /// <summary>
        /// This serial is made for server->client com. NB: SheetId must be initialized.
        /// </summary>
        public static PhraseCom Serial(BitMemoryStream impulse, SheetIdFactory sheetIdFactory)
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
                ret._serialSbrickType = sheetIdFactory.TypeFromFileExtension("sbrick");
            }

            // read
            #region workaround: impulse.SerialCont(ref _serialCompBricks);

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

            #endregion workaround for: ret.Bricks.Resize(serialCompBricks.Count);

            // uncompress
            #region workaround: ContReset(Bricks); Bricks.resize(compBricks.size());

            ret.Bricks.Clear();

            for (var i = 0; i < serialCompBricks.Count; i++)
            {
                ret.Bricks.Add(new SheetId(sheetIdFactory));
            }

            #endregion end workaround

            for (var i = 0; i < ret.Bricks.Count; i++)
            {
                if (serialCompBricks[i] == 0)
                {
                    ret.Bricks[i] = null;
                }
                else
                {
                    ret.Bricks[i].BuildSheetId(serialCompBricks[i] - 1, (EntitySheet.SheetType)ret._serialSbrickType);
                }
            }

            impulse.Serial(ref ret.IconIndex);

            return ret;
        }
    }
}
