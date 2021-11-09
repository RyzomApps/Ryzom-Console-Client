﻿///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

using System;
using System.Diagnostics;
using System.Numerics;
using RCC.Database;
using RCC.Property;

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
        /// Current Name for the entity
        /// </summary>
        private string _entityName;

        /// <summary>
        /// Current guild name of the entity
        /// </summary>
        private string _entityGuildName;

        /// <summary>
        /// Slot of the entity.
        /// </summary>
        internal byte _slot;

        /// <summary>
        /// Slot of the target or CLFECOMMON::INVALID_SLOT if there is no target.
        /// </summary>
        private byte _targetSlot;

        private DatabaseNodeBranch _dbEntry;

        private uint _NameId;
        private uint _GuildNameId;
        private double _HeadPitch;

        public Vector3 Pos { get; set; }

        public Vector3 Front { get; set; }

        public Vector3 Dir { get; set; }

        public void SetHeadPitch(double hp)
        {
            _HeadPitch = hp;
            const double bound = Math.PI / 2 - 0.01; //  epsilon to avoid gimbal lock
            _HeadPitch = Math.Min(Math.Max(_HeadPitch, -bound), bound);
        }

        public void SetName(uint id, string value)
        {
            //RyzomClient.GetInstance().GetLogger().Info($"{_slot} received a name: {value} ({id})");

            _entityName = value;
        }

        private void SetGuildName(uint id, string value)
        {
            //RyzomClient.GetInstance().GetLogger().Info($"{_slot} received a guild name: {value} ({id})");

            _entityGuildName = value;
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

        /// <summary>
        /// Default constructor
        /// </summary>
        public Entity()
        {
            // Initialize the object.
            _type = EntityType.Entity;

            _dataSetId = Constants.InvalidClientDatasetIndex;

            _slot = Constants.InvalidSlot;
            _targetSlot = Constants.InvalidSlot;

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
        /// Return a displayable name
        /// </summary>
        public string GetDisplayName()
        {
            return _entityName == null ? "" : RemoveTitleAndShardFromName(_entityName);
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

        public void SetTargetSlot(byte value)
        {
            _targetSlot = value;
        }

        /// <summary>
        /// Set the slot.
        /// </summary>
        public void SetSlot(in byte slot, DatabaseManager _databaseManager)
        {
            _slot = slot;

            // Get the DB Entry - from CCharacterCL::build
            if (_databaseManager != null && _databaseManager?.GetNodePtr() != null)
            {
                DatabaseNodeBranch nodeRoot = (DatabaseNodeBranch)(_databaseManager.GetNodePtr().GetNode(0));
                if (nodeRoot != null)
                {
                    _dbEntry = (DatabaseNodeBranch)(nodeRoot.GetNode(_slot));
                    if (_dbEntry == null)
                        throw new Exception("Cannot get a pointer on the DB entry.");
                }
            }
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

            if (nodePtr != null)
            {
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
                        UpdateVisualPropertyPos(gameCycle, nodeProp.GetValue64(), predictedInterval);
                        break;

                    case PropertyType.Orientation:
                        //UpdateVisualPropertyOrient(gameCycle, nodeProp.GetValue64());
                        break;

                    case PropertyType.Behaviour:
                        //UpdateVisualPropertyBehaviour(gameCycle, nodeProp.GetValue64());
                        break;

                    case PropertyType.NameStringID:
                        UpdateVisualPropertyName(gameCycle, nodeProp.GetValue64(), client);
                        break;

                    case PropertyType.TargetID:
                        UpdateVisualPropertyTarget(gameCycle, nodeProp.GetValue64());
                        break;

                    case PropertyType.Mode:
                        //UpdateVisualPropertyMode(gameCycle, nodeProp.GetValue64());
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
                        //case PropertyType.TargetList1:
                        //case PropertyType.TargetList2:
                        //case PropertyType.TargetList3:
                        //    //UpdateVisualPropertyTargetList(gameCycle, nodeProp.GetValue64(), prop - PropertyType.TargetList0);
                        break;

                    case PropertyType.VisualFx:
                        //UpdateVisualPropertyVisualFX(gameCycle, nodeProp.GetValue64());
                        break;

                    // Property to update the contextual menu, and some important status
                    case PropertyType.Contextual:
                        //UpdateVisualPropertyContextual(gameCycle, nodeProp.GetValue64());
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
        }

        private void UpdateVisualPropertyBars(uint gameCycle, long prop, RyzomClient client)
        {
            //// Encode HP to 7 bits
            //barInfo.Score[SCORES::hit_points]
            var hitPoints = (byte)((prop & 0x7ff) * 127 / 1023);
            //// NB: barInfo are sint8, but no problem, since anything following is 7 bits.
            //barInfo.Score[SCORES::stamina]
            var stamina = (byte)((prop >> 11) & 0x7f);
            //barInfo.Score[SCORES::sap]
            var sap = (byte)((prop >> 18) & 0x7f);
            //barInfo.Score[SCORES::focus]
            var focus = (byte)((prop >> 25) & 0x7f);

            //client.GetLogger().Info($"{_entityName} {hitPoints} {stamina} {sap} {focus}");
            client.Automata.OnEntityUpdateBars(gameCycle, prop, _slot, hitPoints, stamina, sap, focus);
        }

        /// <summary>
        /// Received the new target for the entity.
        /// </summary>
        private void UpdateVisualPropertyTarget(uint gameCycle, long prop)
        {
            // New target Received.
            int targ = (int)prop;

            // TODO: Workaround without stages
            _targetSlot = (byte)targ;
        }

        /// <summary>
        /// Received the guild name Id.
        /// </summary>
        private void UpdateVisualPropertyGuildNameID(in uint _, long prop, RyzomClient client)
        {
            // Update the entity guild name
            uint guildNameId = (uint)prop;

            // Store the guild name Id
            _GuildNameId = guildNameId;

            client.GetStringManager().WaitString(guildNameId, SetGuildName, client.GetNetworkManager());
        }

        /// <summary>
        /// Received the name Id.
        /// </summary>
        private void UpdateVisualPropertyName(uint _, long prop, RyzomClient client)
        {
            // Update the entity name (do not need to be managed with LCT).
            uint nameId = (uint)prop;

            // Store the name Id
            _NameId = nameId;

            //	STRING_MANAGER::CStringManagerClient::instance()->waitString(nameId, this, &_Name);
            client.GetStringManager().WaitString(nameId, SetName, client.GetNetworkManager());

            //if(!getEntityName().empty())
            //	nlwarning("CH::updateVPName:%d: name Id '%d' received but no name allocated.", _Slot, nameId);
            //else if(verboseVP(this))
            //	nlinfo("(%05d,%03d) CH::updateVPName:%d: name '%s(%d)' received.", sint32(T1%100000), NetMngr.getCurrentServerTick(), _Slot, getEntityName().toString().c_str(), nameId);
            //updateMissionTarget();
        }


        /// <summary>Received a new position for the entity.</summary>
        /// <remarks>Do not send position for the user</remarks> 
        private void UpdateVisualPropertyPos(in uint gameCycle, object prop, in uint pI)
        {
            // Check the DB entry (the warning is already done in the build method).
            if (_dbEntry == null)
            {
                return;
            }

            // Get The property 'Y'.
            if (!(_dbEntry.GetNode((byte)PropertyType.PositionY) is DatabaseNodeLeaf nodeY))
            {
                Debug.Print($"CH::updtVPPos:{_slot}: Cannot find the property 'PROPERTY_POSY({PropertyType.PositionY})'.");
                return;
            }

            // Get The property 'Z'.
            if (!(_dbEntry.GetNode((byte)PropertyType.PositionZ) is DatabaseNodeLeaf nodeZ))
            {
                Debug.Print($"CH::updtVPPos:{_slot}: Cannot find the property 'PROPERTY_POSZ({PropertyType.PositionZ})'.");
                return;
            }

            // Convert Database into a Position
            var x = (float)(Convert.ToDouble(prop) / 1000.0f);
            var y = nodeY.GetValue64() / 1000.0f;
            var z = nodeZ.GetValue64() / 1000.0f;

            Pos = new Vector3(x, y, z);

            //RyzomClient.GetInstance().GetLogger().Info(_slot + " moved to " + Pos);

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
    }
}