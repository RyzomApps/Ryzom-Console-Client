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
using System.Numerics;
using API.Entity;
using API.Sheet;
using Client.Forage;
using Client.Property;
using Client.Sheet;

namespace Client.Entity
{
    /// <summary>
    /// Class to manage entities and shapes instances.
    /// </summary>
    /// <author>Guillaume PUZIN</author>
    /// <author>Nevrax France</author>
    /// <date>2001</date>
    public class EntityManager : IEntityManager
    {
        private readonly RyzomClient _client;

        private uint _nbMaxEntity;

        public UserEntity UserEntity { get; set; }

        /// <inheritdoc />
        public IUserEntity GetApiUserEntity() => UserEntity;

        // Contain all entities.
        private Entity[] _entities;

        private readonly Dictionary<uint, Dictionary<uint, Property>> _backupedChanges = new Dictionary<uint, Dictionary<uint, Property>>();

        /// <inheritdoc />
        public IEntity[] GetApiEntities() => _entities;

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

            _entities = new Entity[_nbMaxEntity];

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
        public Entity Create(in byte slot, uint form, PropertyChange.TNewEntityInfo newEntityInfo)
        {
            var sheetId = _client.GetSheetIdFactory().SheetId(form);

            // DEBUG
            _client.GetLogger().Debug($"(_,{_client.GetNetworkManager().GetCurrentServerTick()}) EM:create: slot '{slot}': {sheetId}");

            if (slot >= _nbMaxEntity)
            {
                _client.GetLogger().Warn($"EM:create: Cannot create the entity, the slot '{slot}' is invalid.");
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

                return null;
            }

            // Remove the old one (except the user).
            if (slot != 0 && _entities[slot] != null)
            {
                Remove(slot, false);
            }

            // Check parameter: form
            var entitySheet = _client.GetSheetManager().Get(sheetId);

            if (entitySheet == null)
            {
                _client.GetLogger().Warn($"EM:create: Attempt on create an entity with a bad form number {form} ({sheetId}) for the slot '{slot}' trying to compute the default one.");

                if (slot != 0)
                {
                    _entities[slot] = new Entity(_client);
                    return null;
                }
                else
                {
                    UserEntity = new UserEntity(_client) { Pos = _client.GetNetworkConnection().GetPropertyDecoder().GetReferencePosition() };
                    _entities[slot] = UserEntity;
                    return null;
                }
            }

            // Create the entity according to the type.
            // TODO: Create the right classes here

            switch (entitySheet.Type)
            {
                case SheetType.RACE_STATS:
                case SheetType.CHAR:
                    if (slot == 0)
                    {
                        UserEntity = new UserEntity(_client) { Pos = _client.GetNetworkConnection().GetPropertyDecoder().GetReferencePosition() };
                        _entities[slot] = UserEntity;
                    }
                    else
                    {
                        _entities[slot] = new PlayerEntity(_client);
                    }

                    break;

                case SheetType.FAUNA:
                    //if (entitySheet is CharacterSheet { R2Npc: false })
                    _entities[slot] = new CharacterEntity(_client);
                    //else
                    // CPlayerR2CL
                    //_entities[slot] = new PlayerEntity(_client) { Type = EntityType.NPC };
                    break;

                case SheetType.FLORA:
                    _entities[slot] = new CharacterEntity(_client);
                    break;

                case SheetType.FX:
                    // TODO: _entities[slot] = new CFxCL;
                    _entities[slot] = new Entity(_client);
                    break;

                case SheetType.ITEM:
                    // TODO: _entities[slot] = new CItemCL;
                    _entities[slot] = new Entity(_client);
                    break;

                case SheetType.FORAGE_SOURCE:
                    _entities[slot] = new ForageSourceEntity(_client);
                    break;

                default:
                    _client.GetLogger().Warn($"Unknown Form Type '{entitySheet.Type}' -> entity not created.");
                    return null;
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

                // Build the entity from a sheet.
                if (_entities[slot].Build((Sheet.Sheet)entitySheet, _client))
                {
                    // Apply properties from backup
                    ApplyBackupProperties(slot);
                }
            }

            _client.Plugins.OnEntityCreate(slot);

            return _entities[slot];
        }

        /// <summary>
        /// Delete an entity.
        /// </summary>
        /// <returns>bool : 'true' if the entity has been correctly removed</returns> 
        public bool Remove(in byte slot, bool warning)
        {
            if (warning)
                _client.Plugins.OnEntityRemove(slot, warning);

            //_client.GetLogger().Info($"EntityManager.Remove({slot}, {warning})");

            // TODO: Implementation
            _entities[slot] = null;

            return true;
        }

        /// <summary>
        /// Method to update the visual property 'prop' for the entity in 'slot'.
        /// </summary>
        /// <param name="gameCycle">timestamp</param>
        /// <param name="slot">slot of the entity to update.</param> 
        /// <param name="prop">the property to udapte.</param>
        /// <param name="predictedInterval">prediction</param> 
        public void UpdateVisualProperty(uint gameCycle, byte slot, uint prop, uint predictedInterval)
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

                if (_client.GetDatabaseManager() != null)
                    propty.Value = _client.GetDatabaseManager().GetProp(propName);

                _client.GetLogger().Debug($"EM:updateVP: backup the property {(PropertyType)prop} as long as the entity {slot} is not allocated.");

                // Entity does not have any changes backuped for the time.
                if (!_backupedChanges.ContainsKey(slot))
                {
                    _backupedChanges.Add(slot, new Dictionary<uint, Property> { { prop, propty } });
                }
                // Entity already have some changes backuped.
                else
                {
                    // This properties is still not backuped for this entity.
                    if (!_backupedChanges[slot].ContainsKey(prop))
                    {
                        _backupedChanges[slot].Add(prop, propty);
                    }
                    // There is already a backuped value
                    else
                    {
                        _client.GetLogger().Debug($"EM:updateVP:{slot}: property '{prop}' already backuped.");
                        _backupedChanges[slot][prop] = propty;
                    }
                }
            }
            // Entity already allocated -> apply values.
            else
            {
                // Call the method from the entity to update the visual property.
                _entities[slot].UpdateVisualProperty(gameCycle, prop, predictedInterval, _client);

                _client.Plugins.OnEntityUpdateVisualProperty(gameCycle, slot, prop, predictedInterval);
            }
        }

        /// <summary>
        /// Writes the properties that were already received before the entity was created to the entity
        /// </summary>
        public void ApplyBackupProperties(uint slot)
        {
            if (!_backupedChanges.ContainsKey(slot))
                return;

            foreach (var (key, property) in _backupedChanges[slot])
            {
                _entities[slot].UpdateVisualProperty(property.GameCycle, key, 0, _client);

                _client.Plugins.OnEntityUpdateVisualProperty(property.GameCycle, (byte)slot, key, 0);
            }

            _backupedChanges.Remove(slot);
        }

        /// <inheritdoc />
        public IEntity GetEntityByName(string name, bool caseSensitive, bool complete)
        {
            var minDistance = float.MaxValue;
            IEntity minEntity = null;

            foreach (var entity in _entities)
            {
                if (entity != null && entity.GetDisplayName().Contains(name, caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase))
                {
                    if (UserEntity == null)
                    {
                        _client.Log.Info($"Found entity '{name}' and user entity is null.");
                        return entity;
                    }

                    // Try to get the closest
                    var distance = Vector3.Distance(entity.Pos, UserEntity.Pos);

                    if (distance < minDistance && distance < 500)
                    {
                        minEntity = entity;
                        minDistance = distance;
                    }
                    else
                    {
                        _client.Log.Debug($"Found entity '{name}' in a distance of {distance:0.0} m (User: {UserEntity.Pos} - Entity: {entity.Pos}).");
                    }
                }
            }

            return minEntity;
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
                var sheet = _entities[i] != null ? _client.GetApiSheetIdFactory()?.SheetId(_entities[i].SheetId()) : null;

                strTmp = _entities[i] == null ? $"// {i}\n" : $"\"{(sheet != null ? sheet.ToString() : _entities[i].SheetId().ToString())}\",\t\"{_entities[i].Pos.X}\", \"{_entities[i].Pos.Y}\", \"{_entities[i].Pos.Z}\", \"{_entities[i].Front.X}\", \"{_entities[i].Front.Y}\", \"{_entities[i].Front.Z}\",\t// {i} {_entities[i].GetDisplayName()} {_entities[i].GetTitle()} {_entities[i].GetEntityType()}\n";
                file.Write(strTmp);
            }

            strTmp = "};\n";
            file.Write(strTmp);

            // Close the File.
            file.Close();
        }

        /// <summary>
        /// Get an entity by dataset index. Returns null if the entity is not found.
        /// </summary>
        /// <param name="compressedIndex">The dataset index to search for.</param>
        /// <returns>An instance of <see cref="Entity"/> if found; otherwise, null.</returns>
        public IEntity GetEntityByCompressedIndex(uint compressedIndex)
        {
            if (compressedIndex == Constants.InvalidClientDatasetIndex)
                return null;

            for (uint i = 0; i < _nbMaxEntity; i++)
            {
                if (_entities[i] != null && _entities[i].DataSetId() == compressedIndex)
                {
                    return _entities[i];
                }
            }

            return null;
        }
    }
}