///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

using RCC.Helper;
using System.Collections.Generic;
using RCC.Property;

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
        private const uint InvalidClientDatasetIndex = 0xFFFFF;

        private readonly RyzomClient _client;

        public UserEntity UserEntity;
        private uint _nbMaxEntity;

        // Contain all entities.
        readonly List<Entity> _entities = new List<Entity>();

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
                _entities.Resize((int)_nbMaxEntity);
                //_EntityGroundFXHandle.resize(_nbMaxEntity);
            }

            // TODO: Add an observer on the mission database
            // TODO: Add an Observer to the Team database
            // TODO: Add an Observer to the Animal database
        }

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

            // Slot 0 is for the user and so should be allocated only once (at beginning of main loop).
            if (slot == 0 && _entities[0] != null)
            {
                if (newEntityInfo.DataSetIndex != InvalidClientDatasetIndex)
                {
                    // Store the dataSetId received
                    _entities[0].DataSetId(newEntityInfo.DataSetIndex);
                }
                // Store the alias (although there should not be one for the slot 0!)
                _entities[0].NpcAlias(newEntityInfo.Alias);

                //_client.Automata.OnEntityCreate(slot, form, newEntityInfo); TODO onplayerentitycreate

                return _entities[0];
            }

            // Remove the old one (except the user).
            _entities[slot] = null;

            // TODO CEntitySheet *entitySheet = SheetMngr.get((CSheetId)form);

            // Create the entity according to the type.
            _entities[slot] = new Entity();

            // If the entity has been right created.
            if (_entities[slot] != null)
            {
                // Set the sheet Id.
                //_Entities[slot].sheetId((CSheetId)form);

                // Set the slot.
                //_entities[slot].slot(slot);

                // Set the DataSet Index. AFTER slot(), so bar manager is correctly init
                _entities[slot].DataSetId(newEntityInfo.DataSetIndex);

                // Set the Mission Giver Alias
                _entities[slot].NpcAlias(newEntityInfo.Alias);
            }

            _client.Automata.OnEntityCreate(slot, form, newEntityInfo);

            // TODO: Implementation

            return _entities[slot];
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
        /// <param name="gameCycle">timestamp</param>
        /// <param name="slot">uint : slot of the entity to update.</param> 
        /// <param name="prop">uint : the property to udapte.</param>
        /// <param name="predictedInterval">prediction</param> 
        public void UpdateVisualProperty(in uint gameCycle, in byte slot, in byte prop, in uint predictedInterval)
        {
            // Check parameter : slot.
            if (slot >= _nbMaxEntity)
            {
                _client.GetLogger().Warn($"CEntityManager::updateVisualProperty : Slot '{slot}' is not valid.");
                return;
            }

            // Entity still not allocated -> backup values received for the entity.
            if (_entities[slot] == null)
            {
                //string propName = "SERVER:Entities:E"+ slot + ":P"+ prop;
                //Property propty = new Property {GameCycle = gameCycle, Value = 0};

                // propty.Value = IngameDbMngr.getProp(propName);


                //TBackupedChanges.iterator it = _BackupedChanges.find(slot);
                //// Entity does not have any changes backuped for the time.
                //if (it == _BackupedChanges.end())
                //{
                //    TProperties propMap = new TProperties();
                //    propMap.insert(Tuple.Create(prop, propty));
                //    _BackupedChanges.insert(Tuple.Create(slot, propMap));
                //}
                //// Entity already have some changes backuped.
                //else
                //{
                //    TProperties properties = it.second;
                //    TProperties.iterator itProp = properties.find(prop);
                //    // This properties is still not backuped for this entity.
                //    if (itProp == properties.end())
                //    {
                //        properties.Add(prop, propty);
                //    }
                //    // There is already a backuped value
                //    else
                //    {
                //        _client.GetLogger().Warn($"EM:updateVP:{slot}: property '{prop}' already backuped.");
                //        itProp.second = propty;
                //    }
                //}
            }
            // Entity already allocated -> apply values.
            else
            {
                // Call the method from the entity to update the visual property.
                _entities[slot].UpdateVisualProperty(gameCycle, prop, predictedInterval);
            }

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