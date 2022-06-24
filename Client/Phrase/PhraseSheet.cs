///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

using Client.Network;
using Client.Sheet;
using Client.Stream;

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
        public PhraseSheet(/*SheetIdFactory sheetIdFactory*/) : base(/*sheetIdFactory*/)
        {
            //_sheetIdFactory = sheetIdFactory;
            _type = SheetType.SPHRASE;
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
                var value = new SheetId();
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
        public bool IsValid()
        {
            if (Bricks.Length == 0)
            {
                return false;
            }

            for (uint i = 0; i < Bricks.Length; i++)
            {
                if (Bricks[i] == SheetId.Unknown)
                {
                    return false;
                }
            }

            return true;
        }

        public override void Serial(BitStreamFile s)
        {
            s.Serial(out uint len);

            Bricks = new SheetId[len];

            for (var i = 0; i < len; i++)
            {
                var value = new SheetId();
                value.Serial(s);
                Bricks[i] = value;
            }

            s.Serial(out Castable);
            s.Serial(out ShowInActionProgression);
            s.Serial(out ShowInApOnlyIfLearnt);
        }

        //private readonly SheetIdFactory _sheetIdFactory;
        public const int SphraseMaxBrick = 100;

        /// <summary>
        /// Build the entity from an external script
        /// TODO: entity?
        /// </summary>
        public bool Build(EntitySheet root)
        {
            //string sTmp;
            //string sTmp2;

            uint i;

            for (i = 0; i < SphraseMaxBrick; ++i)
            {
                //sTmp2 = "brick " + i;

                //root.GetValueByName(sTmp, sTmp2);

                //if (!string.IsNullOrEmpty(sTmp))
                //{
                //    SheetId id = new SheetId(sTmp);
                //    Bricks.Add(id);
                //}
            }

            //// read castable
            //TRANSLATE_VAL(Castable, "castable");

            //// read ShowInActionProgression
            //TRANSLATE_VAL(ShowInActionProgression, "ShowInActionProgression");

            //// read ShowInAPOnlyIfLearnt
            //TRANSLATE_VAL(ShowInAPOnlyIfLearnt, "ShowInAPOnlyIfLearnt");

            return true;
        }
    }
}
