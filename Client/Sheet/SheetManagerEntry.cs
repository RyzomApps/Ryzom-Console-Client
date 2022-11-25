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

            var type = (EntitySheet.SheetType)intType;

            EntitySheet = null;

            switch (type)
            {
                case EntitySheet.SheetType.SBRICK:
                    EntitySheet = new BrickSheet(_client.GetSheetIdFactory());
                    InitSheet(EntitySheet, stream, type);
                    break;

                case EntitySheet.SheetType.SPHRASE:
                    EntitySheet = new PhraseSheet(_client.GetSheetIdFactory());
                    InitSheet(EntitySheet, stream, type);
                    break;

                case EntitySheet.SheetType.FORAGE_SOURCE:
                    EntitySheet = new ForageSourceSheet(_client.GetSheetIdFactory());
                    InitSheet(EntitySheet, stream, type);
                    break;

                case EntitySheet.SheetType.RACE_STATS:
                    EntitySheet = new RaceStatsSheet(_client.GetSheetIdFactory());
                    InitSheet(EntitySheet, stream, type);
                    break;

                case EntitySheet.SheetType.CHAR:
                case EntitySheet.SheetType.FAUNA:
                case EntitySheet.SheetType.FLORA:
                case EntitySheet.SheetType.OBJECT:
                case EntitySheet.SheetType.FX:
                case EntitySheet.SheetType.BUILDING:
                case EntitySheet.SheetType.ITEM:
                case EntitySheet.SheetType.PLANT:
                case EntitySheet.SheetType.MISSION:
                case EntitySheet.SheetType.PACT:
                case EntitySheet.SheetType.LIGHT_CYCLE:
                case EntitySheet.SheetType.WEATHER_SETUP:
                case EntitySheet.SheetType.CONTINENT:
                case EntitySheet.SheetType.WORLD:
                case EntitySheet.SheetType.WEATHER_FUNCTION_PARAMS:
                case EntitySheet.SheetType.UNKNOWN:
                case EntitySheet.SheetType.BOTCHAT:
                case EntitySheet.SheetType.MISSION_ICON:
                case EntitySheet.SheetType.SKILLS_TREE:
                case EntitySheet.SheetType.UNBLOCK_TITLES:
                case EntitySheet.SheetType.SUCCESS_TABLE:
                case EntitySheet.SheetType.AUTOMATON_LIST:
                case EntitySheet.SheetType.ANIMATION_SET_LIST:
                case EntitySheet.SheetType.SPELL:
                case EntitySheet.SheetType.SPELL_LIST:
                case EntitySheet.SheetType.CAST_FX:
                case EntitySheet.SheetType.EMOT:
                case EntitySheet.SheetType.ANIMATION_FX:
                case EntitySheet.SheetType.ID_TO_STRING_ARRAY:
                case EntitySheet.SheetType.CREATURE_ATTACK:
                case EntitySheet.SheetType.ANIMATION_FX_SET:
                case EntitySheet.SheetType.ATTACK_LIST:
                case EntitySheet.SheetType.SKY:
                case EntitySheet.SheetType.TEXT_EMOT:
                case EntitySheet.SheetType.OUTPOST:
                case EntitySheet.SheetType.OUTPOST_SQUAD:
                case EntitySheet.SheetType.OUTPOST_BUILDING:
                case EntitySheet.SheetType.FACTION:
                case EntitySheet.SheetType.TypeCount:
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Useful for serial
        /// </summary>
        public void InitSheet(EntitySheet sheet, BitStreamFile stream, EntitySheet.SheetType type)
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