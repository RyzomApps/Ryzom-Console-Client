///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

using Client.Network;
using System;
using System.Collections.Generic;

namespace Client.Sheet
{
    /// <summary>
    /// Class to manage an entity sheet
    /// </summary>
    /// <author>Guillaume PUZIN</author>
    /// <author>Nevrax France</author>
    /// <date>2001</date>
    public abstract class EntitySheet : IDisposable
    {
        private static readonly List<string> _debug = new List<string>();

        public enum TType
        {
            @sbyte = 0,
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
        public TType _type;

        /// <summary>
        /// Sheet Id
        /// </summary>
        public SheetId Id = new SheetId();

        /// Add string to the debug stack
        //	static void debug(string str);

        /// Flush the debug stack with a title parameter
        //	static void flush(string title);

        /// <summary>
        /// Constructor
        /// </summary>
        public EntitySheet()
        {
            _type = TType.UNKNOWN_SHEET_TYPE;
        }

        /// <summary>
        /// Destructor
        /// </summary>
        public virtual void Dispose()
        {
        }

        /// <summary>
        /// Build the entity from an external script
        /// </summary>
        public abstract void Build(object item);

        /// <summary>
        /// Return the type of the sheet
        /// </summary>
        public TType Type()
        {
            return _type;
        }

        // TType enum/string conversion
        //	static string typeToString(TType e);

        //	static TType typeFromString(string s);

        /// <summary>
        /// Serialize character sheet into binary data file
        /// </summary>
        public abstract void Serial(BitMemoryStream f);
    }
}
