///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

using System;
using Client.Client;
using Client.Stream;

namespace Client.Sheet
{
    /// <summary>
    /// Class to manage .race_stats sheets
    /// </summary>
    public class RaceStatsSheet : Sheet
    {
        private enum Characteristics
        {
            /// <summary>HP max</summary>
            Constitution = 0,
            /// <summary>Hp Regen</summary>
            Metabolism,

            /// <summary>Sap Max</summary>
            Intelligence,
            /// <summary>Sap regen</summary>
            Wisdom,

            /// <summary>Stamina Max</summary>
            Strength,
            /// <summary>Stamina regen</summary>
            WellBalanced,

            /// <summary>Focus Max</summary>
            Dexterity,
            /// <summary>Focus regen</summary>
            Will,

            NumCharacteristics,
            Unknown = NumCharacteristics
        };

        public PeopleType People = PeopleType.Undefined;

        /// <summary>Start characteristics values for this race</summary>
        public byte[] CharacStartValue = new byte[(int)Characteristics.NumCharacteristics];

        /// <summary> Per gender infos. 0 is for male and 1 is for female</summary>
        public GenderInfo[] GenderInfos = new GenderInfo[2];

        /// <summary> Skin to use for this race.</summary>
        public byte Skin;

        /// <summary> Automaton Type</summary>
        public string Automaton = "";

        /// <summary>
        /// Serialize rce_stats sheet into binary data file.
        /// </summary>
        public override void Serial(BitStreamFile f)
        {
            for (uint k = 0; k < (int)Characteristics.NumCharacteristics; ++k)
            {
                f.Serial(out CharacStartValue[k]);
            }

            GenderInfos[0] = new GenderInfo();
            GenderInfos[1] = new GenderInfo();

            GenderInfos[0].Serial(f);
            GenderInfos[1].Serial(f);

            f.Serial(out byte people);
            People = (PeopleType)people;

            // The skin
            f.Serial(out Skin);
            f.Serial(out Automaton);

            // workaround
            f.Serial(out string _);
            f.Serial(out string _);
            f.Serial(out string _);
            f.Serial(out string _);
            f.Serial(out string _);
            f.Serial(out string _);
            f.Serial(out string _);
            f.Serial(out string _);
            f.Serial(out string _);
            f.Serial(out string _);
            // end workaround

            // attack list
            f.Serial(out uint size);
            for (uint k = 0; k < size; ++k)
            {
                f.Serial(out string _);
            }
        }

        public override void Serial(BitMemoryStream f)
        {
            throw new NotImplementedException();
        }

        public RaceStatsSheet(SheetIdFactory sheetIdFactory) : base(sheetIdFactory) { }
    }

    public class GenderInfo
    {
        public void Serial(BitStreamFile f)
        {
            const int numUsedVisualSlots = 7;

            // serial used slots
            for (uint k = 0; k < numUsedVisualSlots; ++k)
            {
                f.Serial(out string _);
            }

            f.Serial(out string _);
            f.Serial(out string _);
            f.Serial(out string _);
            f.Serial(out uint _);
            f.Serial(out uint _);

            // workaround
            f.Serial(out uint len);

            for (uint i = 0; i < len; ++i)
            {
                f.Serial(out uint _);
                f.Serial(out string _);
            }
            // end workaround

            for (uint i = 0; i < 8; ++i)
            {
                f.Serial(out uint _);
                f.Serial(out uint _);
            }

            f.Serial(out uint _);
            f.Serial(out uint _);
            f.Serial(out uint _);
        }
    }
}