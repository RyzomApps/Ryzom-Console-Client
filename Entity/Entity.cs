///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

using System.Numerics;

namespace RCC.Entity
{
    /// <summary>
    /// Interface to manage an Entity in the client side.
    /// </summary>
    /// <author>Guillaume PUZIN</author>
    /// <author>Nevrax France</author>
    /// <date>2001</date>
    public class Entity
    {
        /// <summary>
        /// Entity Id (CLFECOMMON::INVALID_CLIENT_DATASET_INDEX for an invalid one)
        /// </summary>
        private uint _dataSetId;

        /// <summary>
        /// Sheet Id of the entity
        /// </summary>
        private uint _sheetId;

        /// <summary>
        /// Persistent NPC Alias of the entity
        /// </summary>
        private uint _npcAlias;

        /// <summary>
        /// Primitive type
        /// </summary>
        private readonly EntityType _type;

        /// <summary>
        /// gamemaster title code of the entity, if any
        /// </summary>
        private readonly int _gmTitle;

        public Vector3 Pos { get; set; }

        public Vector3 Front { get; set; }

        public Vector3 Dir { get; set; }

        public void SetHeadPitch(int _)
        {
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public Entity()
        {
            // Initialize the object.
            _type = EntityType.Entity;
            _gmTitle = 0xFF;

            Init();
        }

        /// <summary>
        /// Initialize the Object with this function for all constructors
        /// </summary>
        private void Init()
        {
            //_Position = CVectorD(0.f, 0.f, 0.f);
            //
            //// No parent.
            //_Parent = CLFECOMMON::INVALID_SLOT;
            //// No entry for the moment.
            //_DBEntry = 0;
            //
            //// Entity is not flyer at the beginning.
            //_Flyer = false;
            //// Initialize the mode.
            //_Mode = MBEHAV::UNKNOWN_MODE;
            //_TheoreticalMode = MBEHAV::UNKNOWN_MODE;
            //
            //// No DataSetId initiliazed
            _dataSetId = 0xFFFFF;
            //_NPCAlias = 0;
            //// The entity is not in any slot for the time.
            //_Slot = CLFECOMMON::INVALID_SLOT;
            //// The entity has no target for the time.
            //_TargetSlot = CLFECOMMON::INVALID_SLOT;
            //_TargetSlotNoLag = CLFECOMMON::INVALID_SLOT;
            //
            //_Title = "Newbie";
            //_HasReservedTitle = false;
            //_EntityName = "Name";
            //
            //_NameId = 0;
            //
            //_HasMoved = false;
            //_IsInTeam = false;
        }

        /// <summary>
        /// Return the entity Id (persistent as long as the entity is connected) (CLFECOMMON::INVALID_CLIENT_DATASET_INDEX for an invalid one)
        /// </summary>
        public uint DataSetId()
        {
            return _dataSetId;
        }

        /// <summary>
        /// Set the entity Id (persistent as long as the entity is connected) (CLFECOMMON::INVALID_CLIENT_DATASET_INDEX for an invalid one)
        /// </summary>
        public void DataSetId(uint dataSet)
        {
            _dataSetId = dataSet;
        }

        /// <summary>
        /// Return the sheet Id of the entity
        /// </summary>
        public uint SheetId()
        {
            return _sheetId;
        }

        /// <summary>
        /// Set the sheet Id of the entity
        /// </summary>
        public void SheetId(uint id)
        {
            _sheetId = id;
        }

        /// <summary>
        /// Return the persistent NPC alias of entity (0 if N/A)
        /// </summary>
        public uint NpcAlias()
        {
            return _npcAlias;
        }

        /// <summary>
        /// Set the persistent NPC alias of the entity
        /// </summary>
        public void NpcAlias(uint alias)
        {
            _npcAlias = alias;
        }

        /// <summary>
        /// Update a visual property from the database.
        /// </summary>
        /// <param name="gameCycle">when this was sent</param>
        /// <param name="prop">the property to udapte</param>
        /// <param name="predictedInterval">prediction</param>
        public void UpdateVisualProperty(uint gameCycle, uint prop, uint predictedInterval)
        {

        }

        #region Static Methods
        public static string RemoveTitleFromName(string name)
        {
            var p1 = name.IndexOf('$');

            if (p1 == -1)
            {
                return name;
            }

            var p2 = name.IndexOf('$', p1 + 1);

            if (p2 != -1)
            {
                return name.Substring(0, p1) + name[(p2 + 1)..];
            }

            return name.Substring(0, p1);
        }

        public static string RemoveShardFromName(string name)
        {
            // The string must contains a '(' and a ')'
            var p0 = name.IndexOf('(');
            var p1 = name.IndexOf(')');

            if (p0 == -1 || p1 == -1 || p1 <= p0)
                return name;

            // Remove all shard names (hack)
            return name.Substring(0, p0) + name[(p1 + 1)..];
        }

        public static string RemoveTitleAndShardFromName(string name)
        {
            return RemoveTitleFromName(RemoveShardFromName(name));
        }
        #endregion
    }
}
