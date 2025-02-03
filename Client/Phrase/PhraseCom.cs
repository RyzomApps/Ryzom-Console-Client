///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using API.Sheet;
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
        public string Name = string.Empty;

        /// <summary>
        /// List Of SBricks composing the phrase.
        /// </summary>
        public readonly List<SheetId> Bricks = [];

        /// <summary>
        /// Index into Bricks to use as icon (if out of range, then automatic icon selection)
        /// </summary>
        private byte _iconIndex = byte.MaxValue;

        /// <summary>
        /// Empty element
        /// </summary>
        public static readonly PhraseCom EmptyPhrase = new();

        /// <summary>
        /// This serial is made for server->client com. NB: SheetId must be initialized.
        /// </summary>
        public static void Serial(ref PhraseCom phrase, BitMemoryStream stream, SheetIdFactory sheetIdFactory)
        {
            if (stream.IsReading())
                phrase = new PhraseCom();

            stream.Serial(ref phrase.Name, false);

            // Get the type of .sbrick
            if (phrase._serialSbrickType == 0)
            {
                phrase._serialSbrickType = sheetIdFactory.TypeFromFileExtension("sbrick");
            }

            if (stream.IsReading())
            {
                // read
                #region workaround: impulse.SerialCont(ref _serialCompBricks);

                var len = 0;
                stream.Serial(ref len);

                // 16 bits compression of the Bricks
                var serialCompBricks = new List<ushort>(len);

                for (var i = 0; i < len; i++)
                {
                    ushort value = 0;
                    stream.Serial(ref value);
                    serialCompBricks.Add(value);
                }

                #endregion workaround for: ret.Bricks.Resize(serialCompBricks.Count);

                // uncompress
                #region workaround: ContReset(Bricks); Bricks.resize(compBricks.size());

                phrase.Bricks.Clear();

                for (var i = 0; i < serialCompBricks.Count; i++)
                {
                    phrase.Bricks.Add(new SheetId(sheetIdFactory));
                }

                #endregion end workaround

                for (var i = 0; i < phrase.Bricks.Count; i++)
                {
                    if (serialCompBricks[i] == 0)
                    {
                        phrase.Bricks[i] = null;
                    }
                    else
                    {
                        phrase.Bricks[i].BuildSheetId(serialCompBricks[i] - 1, (SheetType)phrase._serialSbrickType);
                    }
                }
            }
            else
            {
                // write
                var serialCompBricks = new List<ushort>();

                // fill default with 0.
                serialCompBricks.Clear();
                #region Workaround compBricks.resize(Bricks.size());
                for (var i = 0; i < phrase.Bricks.Count; i++)
                {
                    serialCompBricks.Add(0);
                }
                #endregion

                // compress
                for (var i = 0; i < phrase.Bricks.Count; i++)
                {
                    // if not empty SheetId
                    if (phrase.Bricks[i].AsInt() == 0)
                        continue;

                    var compId = phrase.Bricks[i].GetShortId();

                    // the sbrick SheetId must be <65535, else error!
                    if (compId >= 65535)
                    {
                        Console.WriteLine($@"ERROR: found a .sbrick SheetId with SubId >= 65535: {phrase.Bricks[i]}");
                        // and leave 0.
                    }
                    else
                    {
                        serialCompBricks[i] = (ushort)(compId + 1);
                    }
                }

                // write
                #region Workaround impulse.serialCont(compBricks);
                var len = serialCompBricks.Count;
                stream.Serial(ref len);

                for (var i = 0; i < len; i++)
                {
                    var value = serialCompBricks[i];
                    stream.Serial(ref value);
                }
                #endregion
            }

            stream.Serial(ref phrase._iconIndex);
        }
    }
}
