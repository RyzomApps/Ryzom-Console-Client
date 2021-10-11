///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
                for (var i = 0; i < _nbMaxEntity; i++)
                    _entities.Add(null);
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
                if (newEntityInfo.DataSetIndex != Constants.InvalidClientDatasetIndex)
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
            if (slot != 0) _entities[slot] = null;

            // TODO CEntitySheet *entitySheet = SheetMngr.get((CSheetId)form); USE SHEET TO CREATE ENTITY

            // Create the entity according to the type.
            if (slot != 0)
                _entities[slot] = new Entity();
            else
            {
                UserEntity = new UserEntity
                {
                    Pos = _client.GetNetworkConnection().GetPropertyDecoder().GetReferencePosition()
                };
                _entities[0] = UserEntity;
            }

            // If the entity has been right created.
            if (_entities[slot] != null)
            {
                // Set the sheet Id.
                _entities[slot].SheetId(form);

                // Set the slot.
                _entities[slot].SetSlot(slot, _client.GetDatabaseManager());

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
                var propName = $"SERVER:Entities:E{slot}:P{prop}";
                var propty = new Property { GameCycle = gameCycle, Value = 0 };

                propty.Value = _client.GetDatabaseManager().GetProp(propName);

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
                _entities[slot].UpdateVisualProperty(gameCycle, prop, predictedInterval, _client);
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
            foreach (var entity in _entities)
            {
                if (entity != null && entity.GetDisplayName().Contains(name, StringComparison.OrdinalIgnoreCase))
                {
                    return entity;
                }
            }

            return null;
        }

        public Entity GetEntity(byte slot)
        {
            return _entities[slot];
        }

        /// <summary>
        /// Write a file with the position of all entities.
        /// </summary>
        public void WriteEntities()
        {
            using var file = new StreamWriter("entities.txt");

            var strTmp = "StartCommands = {\n";
            file.Write(strTmp);

            var nb = _entities.Count();

            for (var i = 1; i < nb; ++i)
            {
                if (_entities[i] == null) continue;

                strTmp = $"\"{_entities[i].SheetId()}\",\t\"{_entities[i].Pos.X}\", \"{_entities[i].Pos.Y}\", \"{_entities[i].Pos.Z}\", \"{_entities[i].Front.X}\", \"{_entities[i].Front.Y}\", \"{_entities[i].Front.Z}\",\t// {i}\n";
                file.Write(strTmp);
            }

            strTmp = "};\n";
            file.Write(strTmp);

            // Close the File.
            file.Close();
        }
    }
}