﻿///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

using System;
using System.Numerics;
using API.Entity;
using Client.Client;
using Client.Database;
using Client.Property;
using Client.Sheet;

namespace Client.Entity
{
    /// <summary>
    /// An entity in the client side
    /// </summary>
    /// <author>Guillaume PUZIN</author>
    /// <author>Nevrax France</author>
    /// <date>2001</date>
    public class Entity : StringWaitCallback, IEntity
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
        protected internal EntityType _type;

        /// <summary>
        /// Current Name for the entity
        /// </summary>
        protected internal string _entityName;

        /// <summary>
        /// Current guild name of the entity
        /// </summary>
        private string _entityGuildName;

        /// <summary>
        /// The title of the entity
        /// </summary>
        private string _title;

        /// <summary>
        /// Slot of the entity
        /// </summary>
        protected byte _slot;

        /// <summary>
        /// Slot of the target or CLFECOMMON::INVALID_SLOT if there is no target.
        /// </summary>
        private byte _targetSlot;

        /// <summary>
        /// Database branch entry for the entity
        /// </summary>
        private DatabaseNodeBranch _dbEntry;

        /// <summary>
        /// Current Name for the entity as String ID
        /// </summary>
        private uint _nameId;

        /// <summary>
        /// Current guild name of the entity as String ID
        /// </summary>
        private uint _guildNameId;

        /// <summary>
        /// Head pitch
        /// </summary>
        private double _headPitch;

        /// <summary>
        /// Flags to know what is possible to do with the entity (selectable, liftable, etc.).
        /// </summary>
        public EntityProperties EntityProperties { get; set; } = new EntityProperties();

        /// <summary>
        /// Position Vector of the entity
        /// </summary>
        public Vector3 Pos { get; set; }

        /// <summary>
        /// Front/Look At vector of the entity
        /// </summary>
        public Vector3 Front { get; set; }

        /// <summary>
        /// Direction the entity is facing
        /// </summary>
        public Vector3 Dir { get; set; }

        /// <summary>
        /// Current mode
        /// </summary>
        protected EntityMode _Mode;

        /// <summary>
        /// Return true if the character is currently dead.
        /// </summary>
        public bool IsDead() { return _Mode == EntityMode.Death; }

        /// <summary>
        /// Theoretical Current Mode (could be different from the current mode).
        /// </summary>
        protected EntityMode _TheoreticalMode;

        /// <summary>
        /// Local DB Branch for this entity
        /// </summary>
        protected DatabaseNodeBranch _DBEntry;

        public void SetHeadPitch(double hp)
        {
            _headPitch = hp;
            // epsilon to avoid gimbaled lock
            const double bound = Math.PI / 2 - 0.01;
            _headPitch = Math.Min(Math.Max(_headPitch, -bound), bound);
        }

        private void SetGuildName(uint id, string value)
        {
            _entityGuildName = value;
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public Entity()
        {
            // Initialize the object.
            _type = EntityType.Entity;

            _dataSetId = Constants.InvalidClientDatasetIndex;

            Init();
        }

        /// <summary>
        /// Initialize the Object with this function for all constructors
        /// </summary>
        private void Init()
        {
            //_Position = CVectorD(0.f, 0.f, 0.f);

            //// No parent.
            //_Parent = CLFECOMMON::INVALID_SLOT;

            //// No entry for the moment.
            //_DBEntry = 0;

            //// Entity is not flying at the beginning.
            //_Flyer = false;

            //// Initialize the mode.
            _Mode = EntityMode.UnknownMode;
            _TheoreticalMode = EntityMode.UnknownMode;

            // No DataSetId initialized
            _dataSetId = 0xFFFFF;
            //_NPCAlias = 0;

            // The entity is not in any slot for the time.
            _slot = Constants.InvalidSlot;

            // The entity has no target for the time.
            _targetSlot = Constants.InvalidSlot;

            //_TargetSlotNoLag = CLFECOMMON::INVALID_SLOT;

            _title = "";

            //_HasReservedTitle = false;

            _entityName = "";

            _nameId = 0;

            //_HasMoved = false;
            //_IsInTeam = false;
        }

        /// <summary>
        /// Primitive type of the entity
        /// </summary>
        public EntityType GetEntityType()
        {
            return _type;
        }

        /// <summary>
        /// Return a displayable name
        /// </summary>
        public string GetDisplayName()
        {
            return _entityName == null ? "" : EntityHelper.RemoveTitleAndShardFromName(_entityName);
        }

        /// <summary>
        /// Returns the name of the entities guild
        /// </summary>
        public string GetGuildName()
        {
            return _entityGuildName ?? "";
        }

        /// <summary>
        /// Returns the title of the entity
        /// </summary>
        public string GetTitle()
        {
            return _title ?? "";
        }

        /// <summary>
        /// Return the current slot for the entity or CLFECOMMON::INVALID_SLOT if the entity is not in any slot.
        /// </summary>
        public byte Slot()
        {
            return _slot;
        }

        /// <summary>
        /// Return the current target of the entity or CLFECOMMON::INVALID_SLOT
        /// </summary>
        public byte TargetSlot()
        {
            return _targetSlot;
        }

        /// <inheritdoc />
        public void SetTargetSlot(byte entityId)
        {
            _targetSlot = entityId;
        }

        /// <summary>
        /// Set the slot.
        /// </summary>
        public void SetSlot(in byte slot, DatabaseManager databaseManager)
        {
            _slot = slot;

            // Get the DB Entry - from CCharacterCL::build
            if (databaseManager?.GetNodePtr() == null) return;

            var nodeRoot = (DatabaseNodeBranch)(databaseManager.GetNodePtr().GetNode(0));

            if (nodeRoot == null) return;

            _dbEntry = (DatabaseNodeBranch)(nodeRoot.GetNode(_slot));

            if (_dbEntry == null)
                throw new Exception("Cannot get a pointer on the DB entry.");
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
        /// <param name="client">Main client</param>
        public void UpdateVisualProperty(uint gameCycle, uint prop, uint predictedInterval, RyzomClient client)
        {
            if (client == null)
                throw new Exception("Update a visual property nees a client.");

            var nodePtr = client.GetDatabaseManager().GetNodePtr();

            if (nodePtr == null) return;

            if (!(nodePtr.GetNode(0) is DatabaseNodeBranch nodeRoot))
            {
                client.GetLogger().Warn($"CEntityCL::UpdateVisualProperty : There is no entry in the DB for entities (current slot {_slot}).");
                return;
            }

            if (!(nodeRoot.GetNode(_slot) is DatabaseNodeBranch nodGrp))
            {
                client.GetLogger().Warn($"CEntityCL::UpdateVisualProperty : Cannot find the entity '{_slot}' in the database.");
                return;
            }

            // Get The property ptr.
            if (!(nodGrp.GetNode((ushort)prop) is DatabaseNodeLeaf nodeProp))
            {
                client.GetLogger().Warn($"CEntityCL::UpdateVisualProperty : Cannot find the property '{prop}' for the slot {_slot}.");
                return;
            }

            switch ((PropertyType)prop)
            {
                case PropertyType.Position:
                    UpdateVisualPropertyPos(gameCycle, nodeProp.GetValue64(), predictedInterval, client);
                    break;

                case PropertyType.Orientation:
                    UpdateVisualPropertyOrient(gameCycle, nodeProp.GetValue64(), client);
                    break;

                case PropertyType.Behaviour:
                    //UpdateVisualPropertyBehaviour(gameCycle, nodeProp.GetValue64());
                    break;

                case PropertyType.NameStringID:
                    UpdateVisualPropertyName(gameCycle, nodeProp.GetValue64(), client);
                    break;

                case PropertyType.TargetID:
                    UpdateVisualPropertyTarget(gameCycle, nodeProp.GetValue64(), client);
                    break;

                case PropertyType.Mode:
                    UpdateVisualPropertyMode(gameCycle, nodeProp.GetValue64(), client);
                    break;

                case PropertyType.Vpa:
                    //UpdateVisualPropertyVpa(gameCycle, nodeProp.GetValue64());
                    break;

                case PropertyType.Vpb:
                    //UpdateVisualPropertyVpb(gameCycle, nodeProp.GetValue64());
                    break;

                case PropertyType.Vpc:
                    //UpdateVisualPropertyVpc(gameCycle, nodeProp.GetValue64());
                    break;

                case PropertyType.EntityMountedID:
                    //UpdateVisualPropertyEntityMounted(gameCycle, nodeProp.GetValue64());
                    break;

                case PropertyType.RiderEntityID:
                    //UpdateVisualPropertyRiderEntity(gameCycle, nodeProp.GetValue64());
                    break;

                case PropertyType.TargetList:
                    //UpdateVisualPropertyTargetList(gameCycle, nodeProp.GetValue64(), prop - PropertyType.TargetList0);
                    break;

                case PropertyType.VisualFx:
                    UpdateVisualPropertyVisualFX(gameCycle, nodeProp.GetValue64(), client);
                    break;

                // Property to update the contextual menu, and some important status
                case PropertyType.Contextual:
                    UpdateVisualPropertyContextual(gameCycle, nodeProp.GetValue64());
                    break;

                case PropertyType.Bars:
                    UpdateVisualPropertyBars(gameCycle, nodeProp.GetValue64(), client);
                    break;

                case PropertyType.GuildSymbol:
                    //UpdateVisualPropertyGuildSymbol(gameCycle, nodeProp.GetValue64());
                    break;

                case PropertyType.GuildNameID:
                    UpdateVisualPropertyGuildNameID(gameCycle, nodeProp.GetValue64(), client);
                    break;

                case PropertyType.EventFactionID:
                    //UpdateVisualPropertyEventFactionID(gameCycle, nodeProp.GetValue64());
                    break;

                case PropertyType.PvpMode:
                    //UpdateVisualPropertyPvpMode(gameCycle, nodeProp.GetValue64());
                    break;

                case PropertyType.PvpClan:
                    //UpdateVisualPropertyPvpClan(gameCycle, nodeProp.GetValue64());
                    break;

                case PropertyType.OwnerPeople:
                    //UpdateVisualPropertyOwnerPeople(gameCycle, nodeProp.GetValue64());
                    break;

                case PropertyType.OutpostInfos:
                    //UpdateVisualPropertyOutpostInfos(gameCycle, nodeProp.GetValue64());
                    break;

                default:
                    client.GetLogger().Warn($"CEntityCL::UpdateVisualProperty : Unknown Property '{(PropertyType)prop}' for the entity in the slot '{_slot}'.");
                    break;
            }
        }

        const int	PROPERTY_POSX				= 0;
        const int	PROPERTY_POSY				= 1;
        const int	PROPERTY_POSZ				= 2;
        const int	PROPERTY_ORIENTATION		= 3; // Theta
        const int	PROPERTY_MODE				= 8;

        /// <summary>
        /// New mode received.
        /// </summary>
        /// <remarks>For the first mode, we must have received the position and orientation (but this should be the case).<br/>
        /// Read the position or orientation from the database when reading the mode (no more updated in updateVisualPropertyPos and updateVisualPropertyOrient).</remarks>
        private void UpdateVisualPropertyMode(in uint gameCycle, long prop, RyzomClient client)
        {
            //if (verboseVP(this))
            //{
            //		client.GetLogger().Info("(%05d,%03d) CH:updtVPMode:%d: '%s(%d)' received.", sint32(T1 % 100000), NetMngr.getCurrentServerTick(), _Slot, modeToString((EntityMode)prop).c_str(), (EntityMode)prop);
            //}

            // New Mode Received : Set the Theoretical Current Mode if different.
            if (_TheoreticalMode != (EntityMode)(prop & 0xffff))
            {
                _TheoreticalMode = (EntityMode)(prop & 0xffff);
            }
            else
            {
                client.GetLogger().Warn($"CH:updtVPMode:{_slot}: The mode '{_TheoreticalMode}({(int)_TheoreticalMode})' sent is the same as the current one.");
                return;
            }

            // If it is the first mode, set the mode.
            if (_Mode == EntityMode.UnknownMode)
            {
                // SET THE FIRST POSITION
                //-----------------------
                // Check the DB entry (the warning is already done in the build method).
                if (_DBEntry == null)
                {
                    return;
                }

                // Get The property 'PROPERTY_POSX'.
                if (!(_DBEntry.GetNode(PROPERTY_POSX) is DatabaseNodeLeaf nodeX))
                {
                    client.GetLogger().Warn("CH::updtVPMode:%d: Cannot find the property 'PROPERTY_POSX(%d)'.", _slot, PROPERTY_POSX);
                    return;
                }

                // Get The property 'PROPERTY_POSY'.
                if (!(_DBEntry.GetNode(PROPERTY_POSY) is DatabaseNodeLeaf nodeY))
                {
                    client.GetLogger().Warn("CH::updtVPMode:%d: Cannot find the property 'PROPERTY_POSY(%d)'.", _slot, PROPERTY_POSY);
                    return;
                }

                // Get The property 'PROPERTY_POSZ'.
                if (!(_DBEntry.GetNode(PROPERTY_POSZ) is DatabaseNodeLeaf nodeZ))
                {
                    client.GetLogger().Warn("CH::updtVPMode:%d: Cannot find the property 'PROPERTY_POSZ(%d)'.", _slot, PROPERTY_POSZ);
                    return;
                }

                //// Next position will no longer be the first one.
                //_First_Pos = false;
                //
                //// Insert the primitive into the world.
                //if (_Primitive)
                //{
                //    _Primitive.insertInWorldImage(dynamicWI);
                //}

                // float makes a few cm error
                var x = (float)(nodeX.GetValue64() / 1000d);
                var y = (float)(nodeY.GetValue64() / 1000d);
                var z = (float)(nodeZ.GetValue64() / 1000d);

                // Set the primitive position.
                //pacsPos(CVectorD(x, y, z));
                Pos = new Vector3(x, y, z);

                // SET THE FIRST ORIENTATION
                //--------------------------
                // Get The property 'PROPERTY_ORIENTATION'.
                if (!(_DBEntry.GetNode(PROPERTY_ORIENTATION) is DatabaseNodeLeaf nodeOri))
                {
                    client.GetLogger().Warn("CH::updtVPMode:%d: Cannot find the property 'PROPERTY_ORIENTATION(%d)'.", _slot, PROPERTY_ORIENTATION);
                    return;
                }

                //C64BitsParts parts = new C64BitsParts();
                //parts.i64[0] = nodeOri.GetValue64();
                //float angleZ = parts.f[0];
                //
                //// server forces the entity orientation even if it cannot turn
                //front(CVector((float)Math.Cos(angleZ), (float)Math.Sin(angleZ), 0.0f), true, true, true);
                //dir(front(), false, false);
                //_TargetAngle = angleZ;
                //
                //if (_Primitive)
                //{
                //    _Primitive.setOrientation(angleZ, dynamicWI);
                //}

                // SET THE FIRST MODE
                //-------------------
                // Set the mode Now
                _Mode = _TheoreticalMode;
                //_ModeWanted = _TheoreticalMode;
                //
                //if ((_Mode == MBEHAV.MOUNT_NORMAL) && (_Rider == CLFECOMMON.INVALID_SLOT))
                //{
                //    _Mode = MBEHAV.NORMAL;
                //    _ModeWanted = MBEHAV.MOUNT_NORMAL;
                //
                //    // See also updateVisualPropertyRiderEntity() for the case when _Rider is received after the mode
                //    computeAutomaton();
                //    computeAnimSet();
                //    setAnim(CAnimationStateSheet.Idle);
                //
                //    // Add the mode to the stage.
                //    _Stages.addStage(gameCycle, PROPERTY_MODE, prop);
                //}
                //
                //computeAutomaton();
                //computeAnimSet();
                //setAnim(CAnimationStateSheet.Idle);
            }
            // Not the first mode -> Add to a stage.
            else
            {
                // Add the mode to the stage.
                //_Stages.addStage(gameCycle, PROPERTY_MODE, prop);

                // TODO: workaround - set the mode instantly
                _Mode = _TheoreticalMode;

                // Float mode push the orientation
                if (_TheoreticalMode == EntityMode.CombatFloat)
                {
                    // Get The property 'PROPERTY_ORIENTATION'.

                    if (!(_DBEntry.GetNode(PROPERTY_ORIENTATION) is DatabaseNodeLeaf nodeOri))
                    {
                        client.GetLogger().Warn($"CH::updtVPMode:{_slot}: Cannot find the property 'PROPERTY_ORIENTATION({PROPERTY_ORIENTATION})'.");
                        return;
                    }

                    //_Stages.addStage(gameCycle, PROPERTY_ORIENTATION, nodeOri.GetValue64());
                }
                // Any other mode push the position
                else
                {
                    if (_TheoreticalMode != EntityMode.MountNormal)
                    {
                        // Check the DB entry (the warning is already done in the build method).
                        if (_DBEntry == null)
                        {
                            return;
                        }

                        // Get The property 'PROPERTY_POSX'.
                        if (!(_DBEntry.GetNode(PROPERTY_POSX) is DatabaseNodeLeaf nodeX))
                        {
                            client.GetLogger().Warn("CH::updtVPMode:%d: Cannot find the property 'PROPERTY_POSX(%d)'.", _slot, PROPERTY_POSX);
                            return;
                        }

                        // Get The property 'PROPERTY_POSY'.
                        if (!(_DBEntry.GetNode(PROPERTY_POSY) is DatabaseNodeLeaf nodeY))
                        {
                            client.GetLogger().Warn("CH::updtVPMode:%d: Cannot find the property 'PROPERTY_POSY(%d)'.", _slot, PROPERTY_POSY);
                            return;
                        }

                        // Get The property 'PROPERTY_POSZ'.
                        if (!(_DBEntry.GetNode(PROPERTY_POSZ) is DatabaseNodeLeaf nodeZ))
                        {
                            client.GetLogger().Warn("CH::updtVPMode:%d: Cannot find the property 'PROPERTY_POSZ(%d)'.", _slot, PROPERTY_POSZ);
                            return;
                        }

                        // Add Stage.
                        //_Stages.addStage(gameCycle, CLFECOMMON.PROPERTY_POSX, nodeX.getValue64());
                        //_Stages.addStage(gameCycle, CLFECOMMON.PROPERTY_POSY, nodeY.getValue64());
                        //_Stages.addStage(gameCycle, CLFECOMMON.PROPERTY_POSZ, nodeZ.getValue64());
                    }
                }
            }

        }

        /// <summary>
        /// updateVisualPropertyContextual
        /// </summary>
        public void UpdateVisualPropertyContextual(uint gameCycle, long prop)
        {
            EntityProperties = new EntityProperties((ushort)prop);
        }


        /// <summary>
        /// Update Entity Bars
        /// </summary>
        protected virtual void UpdateVisualPropertyBars(uint gameCycle, long prop, RyzomClient client)
        {
            // Encode HP to 7 bits
            var hitPoints = (sbyte)((prop & 0x7ff) * 127 / 1023);

            // NB: barInfo are sint8, but no problem, since anything following is 7 bits.
            var stamina = (byte)((prop >> 11) & 0x7f);
            var sap = (byte)((prop >> 18) & 0x7f);
            var focus = (byte)((prop >> 25) & 0x7f);

            client.Plugins.OnEntityUpdateBars(gameCycle, prop, _slot, hitPoints, stamina, sap, focus);
        }

        /// <summary>
        /// Received the new target for the entity.
        /// </summary>
        protected virtual void UpdateVisualPropertyTarget(uint _, long prop, RyzomClient client)
        {
            // New target Received.
            var targ = (int)prop;

            // TODO: Workaround without stages
            _targetSlot = (byte)targ;
        }

        /// <summary>
        /// Received the guild name Id.
        /// </summary>
        private void UpdateVisualPropertyGuildNameID(uint _, long prop, RyzomClient client)
        {
            // Update the entity guild name
            var guildNameId = (uint)prop;

            // Store the guild name Id
            _guildNameId = guildNameId;

            client.GetStringManager().WaitString(guildNameId, SetGuildName, client.GetNetworkManager());
        }

        /// <summary>
        /// Received the name Id.
        /// </summary>
        protected virtual void UpdateVisualPropertyName(uint _, long prop, RyzomClient client)
        {
            // Update the entity name (do not need to be managed with LCT).
            var nameId = (uint)prop;

            // Store the name Id
            _nameId = nameId;

            client.GetStringManager().WaitString(nameId, this, client.GetNetworkManager());

            // if(GetEntityName().empty())
            // 	client.GetLogger().Warn("CH::updateVPName:%d: name Id '%d' received but no name allocated.", _Slot, nameId);
            // else if(verboseVP(this))
            // 	client.GetLogger().Info("(%05d,%03d) CH::updateVPName:%d: name '%s(%d)' received.", sint32(T1%100000), NetMngr.getCurrentServerTick(), _Slot, getEntityName().toString().c_str(), nameId);

            // TODO: updateMissionTarget();
        }


        /// <summary>Received a new position for the entity.</summary>
        /// <remarks>Do not send position for the user</remarks> 
        private void UpdateVisualPropertyPos(uint gameCycle, long prop, uint predictedInterval, RyzomClient client)
        {
            // Check the DB entry (the warning is already done in the build method).
            if (_dbEntry == null)
            {
                return;
            }

            // Get The property 'Y'.
            if (!(_dbEntry.GetNode((byte)PropertyType.PositionY) is DatabaseNodeLeaf nodeY))
            {
                client.Log.Debug($"CH::updtVPPos:{_slot}: Cannot find the property 'PROPERTY_POSY({PropertyType.PositionY})'.");
                return;
            }

            // Get The property 'Z'.
            if (!(_dbEntry.GetNode((byte)PropertyType.PositionZ) is DatabaseNodeLeaf nodeZ))
            {
                client.Log.Debug($"CH::updtVPPos:{_slot}: Cannot find the property 'PROPERTY_POSZ({PropertyType.PositionZ})'.");
                return;
            }

            // Convert Database into a Position
            var x = (float)(Convert.ToDouble(prop) / 1000.0f);
            var y = nodeY.GetValue64() / 1000.0f;
            var z = nodeZ.GetValue64() / 1000.0f;

            Pos = new Vector3(x, y, z);

            client.Plugins.OnEntityUpdatePos(gameCycle, prop, _slot, predictedInterval, Pos);

            //// First position Managed -> set the PACS Position
            //if (_FirstPosManaged)
            //{
            //    pacsPos(CVectorD(x, y, z));
            //    _FirstPosManaged = false;
            //    return;
            //}
            //
            //// Wait for the entity to be spawned
            //if (_First_Pos)
            //{
            //    return;
            //}
            //
            //// Stock the position (except if this is the user mount because it's the user that control him not the server)
            //if (!isRiding() || _Rider != 0)
            //{
            //    // Adjust the Predicted Interval to fix some "bug" into the Prediction Algo.
            //    NLMISC.TGameCycle adjustedPI = adjustPI(x, y, z, pI);
            //    // Add Stage.
            //    _Stages.addStage(gameCycle, PROPERTY_POSX, prop, adjustedPI);
            //    _Stages.addStage(gameCycle, PROPERTY_POSY, nodeY.getValue64());
            //    _Stages.addStage(gameCycle, PROPERTY_POSZ, nodeZ.getValue64());
            //}
        }

        /// <summary>
        /// Received a new orientation.
        /// </summary>
        protected virtual unsafe void UpdateVisualPropertyOrient(uint gameCycle, long prop, RyzomClient client)
        {
            // TODO: Implement properly
            var ori = *(float*)&prop;
            Front = new Vector3((float)Math.Cos(ori), (float)Math.Sin(ori), 0.0f);
            Dir = Front;

            client.Plugins.OnEntityUpdateOrient(gameCycle, prop);
        }

        /// <summary>
        /// Build the entity from an external script
        /// </summary>
        public virtual bool Build(EntitySheet sheet, RyzomClient client)
        {
            return true;
        }

        protected virtual void UpdateVisualPropertyVisualFX(uint _, long prop, RyzomClient client)
        {
            // TODO: Not implemented for base entity
        }

        public override void OnDynStringAvailable(uint stringId, string value)
        {
            // TODO: Not implemented for base entity
        }

        /// <summary>
        /// Override for string reception callback
        /// </summary>
        public override void OnStringAvailable(uint stringId, in string value)
        {
            _entityName = value;

            // remove the shard name if possible
            _entityName = EntityHelper.RemoveShardFromName(_entityName);

            // check if there is any replacement tag in the string
            var p1 = _entityName.IndexOf("$", StringComparison.Ordinal);

            if (p1 == -1)
                return;

            // we found a replacement point begin tag
            var p2 = _entityName.IndexOf('$', p1 + 1);

            if (p2 != -1)
            {
                _title = _entityName.Substring(p1 + 1, p2 - p1 - 1);
            }
            else
            {
                _entityName = RyzomClient.GetInstance().GetStringManager().GetLocalizedName(_entityName);
            }
        }
    }
}
