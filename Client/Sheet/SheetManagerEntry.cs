///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

using Client.Brick;
using Client.Phrase;
using System;
using API.Sheet;
using Client.Forage;
using Client.Stream;

namespace Client.Sheet
{
    /// <summary>
    /// Class to manage all sheets.
    /// </summary>
    /// <author>Guillaume PUZIN</author>
    /// <author>Nevrax France</author>
    /// <date>2001</date>
    public class SheetManagerEntry
    {
        private readonly RyzomClient _client;

        /// <summary>
        /// The data which will be filled in readGeorges and serial
        /// </summary>
        public EntitySheet EntitySheet { get; internal set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public SheetManagerEntry(RyzomClient client)
        {
            _client = client;
        }

        /// <summary>
        /// Load/Save the values using the serial system
        /// </summary>
        public void Serial(BitStreamFile stream)
        {
            stream.Serial(out uint intType);

            var type = (SheetType)intType;

            EntitySheet = null;

            switch (type)
            {
                case SheetType.FAUNA:
                    EntitySheet = new CharacterSheet(_client.GetSheetIdFactory());
                    InitSheet(EntitySheet, stream, type);
                    break;

                case SheetType.CHAR:
                    EntitySheet = new PlayerSheet(_client.GetSheetIdFactory());
                    InitSheet(EntitySheet, stream, type);
                    break;

                case SheetType.SBRICK:
                    EntitySheet = new BrickSheet(_client.GetSheetIdFactory());
                    InitSheet(EntitySheet, stream, type);
                    break;

                case SheetType.SPHRASE:
                    EntitySheet = new PhraseSheet(_client.GetSheetIdFactory());
                    InitSheet(EntitySheet, stream, type);
                    break;

                case SheetType.FORAGE_SOURCE:
                    EntitySheet = new ForageSourceSheet(_client.GetSheetIdFactory());
                    InitSheet(EntitySheet, stream, type);
                    break;

                case SheetType.RACE_STATS:
                    EntitySheet = new RaceStatsSheet(_client.GetSheetIdFactory());
                    InitSheet(EntitySheet, stream, type);
                    break;

                case SheetType.FLORA:
                case SheetType.OBJECT:
                case SheetType.FX:
                case SheetType.BUILDING:
                case SheetType.ITEM:
                case SheetType.PLANT:
                case SheetType.MISSION:
                case SheetType.PACT:
                case SheetType.LIGHT_CYCLE:
                case SheetType.WEATHER_SETUP:
                case SheetType.CONTINENT:
                case SheetType.WORLD:
                case SheetType.WEATHER_FUNCTION_PARAMS:
                case SheetType.UNKNOWN:
                case SheetType.BOTCHAT:
                case SheetType.MISSION_ICON:
                case SheetType.SKILLS_TREE:
                case SheetType.UNBLOCK_TITLES:
                case SheetType.SUCCESS_TABLE:
                case SheetType.AUTOMATON_LIST:
                case SheetType.ANIMATION_SET_LIST:
                case SheetType.SPELL:
                case SheetType.SPELL_LIST:
                case SheetType.CAST_FX:
                case SheetType.EMOT:
                case SheetType.ANIMATION_FX:
                case SheetType.ID_TO_STRING_ARRAY:
                case SheetType.CREATURE_ATTACK:
                case SheetType.ANIMATION_FX_SET:
                case SheetType.ATTACK_LIST:
                case SheetType.SKY:
                case SheetType.TEXT_EMOT:
                case SheetType.OUTPOST:
                case SheetType.OUTPOST_SQUAD:
                case SheetType.OUTPOST_BUILDING:
                case SheetType.FACTION:
                case SheetType.TypeCount:
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Useful for serial
        /// </summary>
        public void InitSheet(EntitySheet sheet, BitStreamFile stream, SheetType type)
        {
            if (sheet != null)
            {
                sheet.Id.Serial(stream);
                sheet.Serial(stream);
                sheet.Type = type;
                _client.GetSheetManager().ProcessSheet(sheet);
            }
        }
    }
}