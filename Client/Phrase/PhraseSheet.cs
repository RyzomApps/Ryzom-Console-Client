﻿///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

using Client.Network;
using Client.Sheet;

namespace Client.Phrase
{
    /// <summary>
    /// New Sabrina Brick Sheet def.
    /// </summary>
    /// <author>Matthieu Besson</author>
    /// <author>Nevrax France</author>
    /// <date>2003 September</date>
    public class PhraseSheet : EntitySheet
    {
        private readonly SheetIdFactory _sheetIdFactory;
        public const int SPHRASE_MAX_BRICK = 100;

        // <summary>
        // All these values are sheet id
        // </summary>
        public SheetId[] Bricks = new SheetId[0];

        // <summary>
        // False if it is an upgrade phrase for instance. true means it can be memorized
        // </summary>
        public bool Castable;

        // <summary>
        // True if it can be shown in the ActionProgression window
        // </summary>
        public bool ShowInActionProgression;

        // <summary>
        // True if it can be shown in the ActionProgression window (only if all bricks learn)
        // </summary>
        public bool ShowInApOnlyIfLearnt;

        /// <summary>
        /// Constructor
        /// </summary>
        public PhraseSheet(SheetIdFactory sheetIdFactory) : base(sheetIdFactory)
        {
            _sheetIdFactory = sheetIdFactory;
            _type = TType.SPHRASE;
            Castable = true;
            ShowInActionProgression = true;
            ShowInApOnlyIfLearnt = false;
        }

        /// <summary>
        /// Serialize character sheet into binary data file
        /// </summary>
        public override void Serial(BitMemoryStream s)
        {
            // workaround for: s.SerialCont(ref Bricks);
            var len = 0;
            s.Serial(ref len);

            Bricks = new SheetId[len];

            for (var i = 0; i < len; i++)
            {
                var value = new SheetId(_sheetIdFactory);
                value.Serial(s);
                Bricks[i] = value;
            }

            s.Serial(ref Castable);
            s.Serial(ref ShowInActionProgression);
            s.Serial(ref ShowInApOnlyIfLearnt);
        }

        /// <summary>
        /// Valid if Bricks not empty and all Bricks sheetId != NULL
        /// </summary>
        public bool IsValid(SheetIdFactory sheetIdFactory)
        {
            if (Bricks.Length == 0)
            {
                return false;
            }

            for (uint i = 0; i < Bricks.Length; i++)
            {
                if (Bricks[i] == sheetIdFactory.Unknown)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Build the entity from an external script
        /// </summary>
        public override void Build(object root)
        {
            //string sTmp;
            //string sTmp2;

            uint i;

            for (i = 0; i < SPHRASE_MAX_BRICK; ++i)
            {
                //sTmp2 = "brick " + i;
                //
                //root.GetValueByName(sTmp, sTmp2);
                //
                //if (!string.IsNullOrEmpty(sTmp))
                //{
                //    SheetId id = new SheetId(sTmp);
                //    Bricks.Add(id);
                //}
            }

            //// read castable
            //TRANSLATE_VAL(Castable, "castable");
            //
            //// read ShowInActionProgression
            //TRANSLATE_VAL(ShowInActionProgression, "ShowInActionProgression");
            //
            //// read ShowInAPOnlyIfLearnt
            //TRANSLATE_VAL(ShowInAPOnlyIfLearnt, "ShowInAPOnlyIfLearnt");

        }

        public override void Serial(BitStreamFile s)
        {
            uint len = 0;
            s.Serial(out len);

            Bricks = new SheetId[len];

            for (var i = 0; i < len; i++)
            {
                var value = new SheetId(_sheetIdFactory);
                value.Serial(s);
                Bricks[i] = value;
            }

            s.Serial(out Castable);
            s.Serial(out ShowInActionProgression);
            s.Serial(out ShowInApOnlyIfLearnt);
        }
    }
}
