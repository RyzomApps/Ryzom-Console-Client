///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

using Client.Stream;

namespace Client.Sheet
{
    /// <summary>
    /// Class to manage an entity sheet
    /// </summary>
    /// <author>Guillaume PUZIN</author>
    /// <author>Nevrax France</author>
    /// <date>2001</date>
    public abstract class EntitySheet
    {
        //private readonly SheetIdFactory _sheetIdFactory;

        public enum SheetType
        {
            CHAR = 0,
            FAUNA,
            FLORA,
            OBJECT,
            FX,
            BUILDING,
            ITEM,
            PLANT,
            MISSION,
            RACE_STATS,
            PACT,
            LIGHT_CYCLE,
            WEATHER_SETUP,
            CONTINENT,
            WORLD,
            WEATHER_FUNCTION_PARAMS,
            UNKNOWN,
            BOTCHAT,
            MISSION_ICON,
            SBRICK,
            SPHRASE,
            SKILLS_TREE,
            UNBLOCK_TITLES,
            SUCCESS_TABLE,
            AUTOMATON_LIST,
            ANIMATION_SET_LIST,
            SPELL, // obsolete
            SPELL_LIST, // obsolete
            CAST_FX, // obsolete
            EMOT,
            ANIMATION_FX,
            ID_TO_STRING_ARRAY,
            FORAGE_SOURCE,
            CREATURE_ATTACK,
            ANIMATION_FX_SET,
            ATTACK_LIST,
            SKY,
            TEXT_EMOT,
            OUTPOST,
            OUTPOST_SQUAD,
            OUTPOST_BUILDING,
            FACTION,
            TypeCount,
            UNKNOWN_SHEET_TYPE = TypeCount
        }

        /// <summary>
        /// Type of the sheet
        /// </summary>
        protected SheetType _type;

        /// <summary>
        /// Sheet Id
        /// </summary>
        public SheetId Id;

        /// <summary>
        /// Constructor
        /// </summary>
        protected EntitySheet(SheetIdFactory sheetIdFactory)
        {
            //_sheetIdFactory = sheetIdFactory;
            _type = SheetType.UNKNOWN_SHEET_TYPE;
            Id = new SheetId(sheetIdFactory);
        }

        /// <summary>
        /// Return the type of the sheet
        /// </summary>
        public SheetType Type
        {
            get => _type;
            set => _type = value;
        }

        /// <summary>
        /// Serialize character sheet into binary data file
        /// </summary>
        public abstract void Serial(BitMemoryStream f);

        public abstract void Serial(BitStreamFile s);
    }
}
