///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

using RCC.Helper;
using System.Collections.Generic;

namespace RCC.Entity
{
    /// <summary>
    /// Class to manage entities and shapes instances.
    /// </summary>
    /// <author>Guillaume PUZIN</author>
    /// <author>Nevrax France</author>
    /// <date>2001</date>
    /// 
    public class EntityManager
    {
        private readonly RyzomClient _client;

        public UserEntity UserEntity;
        private uint _nbMaxEntity;

        // Contain all entities.
        List<Entity> _Entities = new List<Entity>();

        public EntityManager(RyzomClient client)
        {
            _client = client;
        }

        /// <summary>
        /// Initialize some dynamic parameters.
        /// </summary>
        /// <param name="nbMaxEntity">uint : maximum number of entities allocated</param> 
        public void Initialize(uint nbMaxEntity)
        {
            // Set the maximum number of entities.
            _nbMaxEntity = nbMaxEntity;

            if (_nbMaxEntity > 0)
            {
                _Entities.Resize((int)_nbMaxEntity);
                //_EntityGroundFXHandle.resize(_nbMaxEntity);
            }

            // TODO: Add an observer on the mission database
            // TODO: Add an Observer to the Team database
            // TODO: Add an Observer to the Animal database
        }

        const uint INVALID_CLIENT_DATASET_INDEX = 0xFFFFF;

        /// <summary>
        /// Create an entity according to the slot and the form.
        /// </summary>
        /// <param name="slot">uint : slot for the entity</param> 
        /// <param name="form">uint32 : form to create the entity</param>
        /// <param name="newEntityInfo">dataset</param>
        /// <returns>CEntityCL : pointer on the new entity</returns> 
        public Entity Create(in byte slot, uint form, Change.TNewEntityInfo newEntityInfo)
        {
            if (slot >= _nbMaxEntity)
            {
                _client.GetLogger().Warn("EM:create: Cannot create the entity, the slot '" + slot + "' is invalid.");
                return null;
            }
            else
            {
                // Slot 0 is for the user and so should be allocated only once (at beginning of main loop).
                if (slot == 0 && _Entities[0] != null)
                {
                    if (newEntityInfo.DataSetIndex != INVALID_CLIENT_DATASET_INDEX)
                    {
                        // Store the dataSetId received
                        _Entities[0].dataSetId(newEntityInfo.DataSetIndex);
                    }
                    // Store the alias (although there should not be one for the slot 0!)
                    _Entities[0].npcAlias(newEntityInfo.Alias);

                    //_client.Automata.OnEntityCreate(slot, form, newEntityInfo); TODO onplayerentitycreate

                    return _Entities[0];
                }
            }

            // Remove the old one (except the user).
            if (_Entities[slot] != null)
            {
                _Entities[slot] = null;
            }

            // TODO CEntitySheet *entitySheet = SheetMngr.get((CSheetId)form);
            _Entities[slot] = new Entity();

            // If the entity has been right created.
            if (_Entities[slot] != null)
            {
                // Set the sheet Id.
                //_Entities[slot].sheetId((CSheetId)form);
                // Set the slot.
                //_Entities[slot].slot(slot);
                // Set the DataSet Index. AFTER slot(), so bar manager is correctly init
                _Entities[slot].dataSetId(newEntityInfo.DataSetIndex);
                // Set the Mission Giver Alias
                _Entities[slot].npcAlias(newEntityInfo.Alias);
            }

            _client.Automata.OnEntityCreate(slot, form, newEntityInfo);

            // TODO: Implementation

            return _Entities[slot];
        }

        /// <summary>
        /// Delete an entity.
        /// </summary>
        /// <returns>bool : 'true' if the entity has been correctly removed</returns> 
        public bool Remove(in byte slot, bool warning)
        {
            _client.Automata.OnEntityRemove(slot, warning);

            // TODO: Implementation

            return true;
        }

        /// <summary>
        /// Method to update the visual property 'prop' for the entity in 'slot'.
        /// </summary>
        /// <param name="slot">uint : slot of the entity to update.</param> 
        /// <param name="prop">uint : the property to udapte.</param> 
        public void UpdateVisualProperty(in uint gameCycle, in byte slot, in byte prop, in uint predictedInterval)
        {
            _client.Automata.OnEntityUpdateVisualProperty(gameCycle, slot, prop, predictedInterval);

            // TODO: Implementation
        }

        /// <summary>
        /// Get an entity by name. Returns NULL if the entity is not found.
        /// </summary>
        /// <param name="name">of the entity to find</param>  
        /// <param name="caseSensitive">type of test to perform</param>  
        /// <param name="complete">if true, the name must match the full name of the entity</param>
        public Entity GetEntityByName(string name, bool caseSensitive, bool complete)
        {
            throw new System.NotImplementedException();
        }
    }
}