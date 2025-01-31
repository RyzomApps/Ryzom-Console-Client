///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

using System;
using System.Diagnostics;
using System.Numerics;
using API;
using API.Entity;
using Client.Client;
using Client.Database;
using Client.Property;
using Client.Sheet;
using Client.Strings;

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
        protected internal EntityType Type;

        /// <summary>
        /// Current Name for the entity
        /// </summary>
        protected internal string _entityName = "";

        /// <summary>
        /// Current guild name of the entity
        /// </summary>
        private string _entityGuildName;

        /// <summary>
        /// The title of the entity
        /// </summary>
        private string _title = "";

        /// <summary>
        /// Slot of the entity
        /// </summary>
        protected byte _slot;

        /// <summary>
        /// Slot of the target or CLFECOMMON::INVALID_SLOT if there is no target.
        /// </summary>
        private byte _targetSlot;

        /// <summary>
        /// Current Name for the entity as String ID
        /// </summary>
        internal uint _nameId;

        /// <summary>
        /// Current guild name of the entity as String ID
        /// </summary>
        private uint _guildNameId;

        /// <summary>
        /// Head pitch
        /// </summary>
        private double _headPitch;

        /// <summary>
        /// Flags to know what is possible to do with the entity (selectable, lift-able, etc.).
        /// </summary>
        public EntityProperties Properties { get; set; } = new EntityProperties();

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
        protected EntityMode Mode;

        /// <summary>
        /// Return true if the character is currently dead.
        /// </summary>
        public bool IsDead() { return Mode == EntityMode.Death; }

        /// <summary>
        /// Theoretical Current Mode (could be different from the current mode).
        /// </summary>
        protected EntityMode TheoreticalMode;

        /// <summary>
        /// Local DB Branch for this entity
        /// </summary>
        protected DatabaseNodeBranch DbEntry;

        /// <summary>
        /// 'true' as long as the entity has not received any position.
        /// </summary>
        protected bool _firstPos;

        /// <summary>
        /// Ryzom Client
        /// </summary>
        protected RyzomClient _client;

        /// <summary>
        /// Default constructor
        /// </summary>
        public Entity(RyzomClient client)
        {
            _client = client;

            // Initialize the object.
            Init();

            Type = EntityType.Entity;

            //_GMTitle = _InvalidGMTitleCode;
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
            //_DBEntry = null;

            //// Entity is not flying at the beginning.
            //_Flyer = false;

            //// Initialize the mode.
            Mode = EntityMode.UnknownMode;
            TheoreticalMode = EntityMode.UnknownMode;

            // No DataSetId initialized
            _dataSetId = 0xFFFFF;
            //_NPCAlias = 0;

            // The entity is not in any slot for the time.
            _slot = Constants.InvalidSlot;

            // The entity has no target for the time.
            _targetSlot = Constants.InvalidSlot;

            //_TargetSlotNoLag = CLFECOMMON::INVALID_SLOT;

            //_HasReservedTitle = false;

            _nameId = 0;

            //_HasMoved = false;
            //_IsInTeam = false;
        }

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
        /// Primitive type of the entity
        /// </summary>
        public EntityType GetEntityType()
        {
            return Type;
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
        public void SetTargetSlot(byte slot)
        {
            _targetSlot = slot;
        }

        /// <summary>
        /// Set the slot.
        /// </summary>
        public void SetSlot(in byte slot, DatabaseManager databaseManager)
        {
            _slot = slot;

            // Get the DB Entry - from CCharacterCL::build
            if (databaseManager?.GetServerDb() == null)
                return;

            var nodeRoot = (DatabaseNodeBranch)(databaseManager.GetServerDb().GetNode(0));

            if (nodeRoot == null)
                return;

            DbEntry = (DatabaseNodeBranch)(nodeRoot.GetNode(_slot));

            if (DbEntry == null)
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

            if (_client.GetDatabaseManager() == null)
                return;

            var nodePtr = client.GetDatabaseManager().GetServerDb();

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


        /// <summary>
        /// updateVisualPropertyContextual
        /// </summary>
        public void UpdateVisualPropertyContextual(uint gameCycle, long prop)
        {
            Properties = new EntityProperties((ushort)prop);
        }

        /// Update Entity Mode.
        protected virtual void UpdateVisualPropertyMode(uint gameCycle, long prop, IClient client)
        {
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
        /// Update Entity Name.
        /// </summary>
        protected virtual void UpdateVisualPropertyName(uint _, long prop, RyzomClient client) { }


        /// <summary>Received a new position for the entity.</summary>
        /// <remarks>Do not send position for the user</remarks> 
        private void UpdateVisualPropertyPos(uint gameCycle, long prop, uint predictedInterval, RyzomClient client)
        {
            // Check the DB entry (the warning is already done in the build method).
            if (DbEntry == null)
            {
                return;
            }

            // Get The property 'Y'.
            if (!(DbEntry.GetNode((byte)PropertyType.PositionY) is DatabaseNodeLeaf nodeY))
            {
                client.Log.Debug($"CH::updtVPPos:{_slot}: Cannot find the property 'PROPERTY_POSY({PropertyType.PositionY})'.");
                return;
            }

            // Get The property 'Z'.
            if (!(DbEntry.GetNode((byte)PropertyType.PositionZ) is DatabaseNodeLeaf nodeZ))
            {
                client.Log.Debug($"CH::updtVPPos:{_slot}: Cannot find the property 'PROPERTY_POSZ({PropertyType.PositionZ})'.");
                return;
            }

            // Convert Database into a Position
            var x = Convert.ToSingle(prop) / 1000.0f;
            var y = Convert.ToSingle(nodeY.GetValue64()) / 1000.0f;
            var z = Convert.ToSingle(nodeZ.GetValue64()) / 1000.0f;

            Pos = new Vector3(x, y, z);

            client.Plugins.OnEntityUpdatePos(gameCycle, prop, _slot, predictedInterval, Pos);

            //// First position Managed -> set the PACS Position
            //if (_FirstPosManaged)
            //{
            //    pacsPos(CVectorD(x, y, z));
            //    _FirstPosManaged = false;
            //    return;
            //}

            // Wait for the entity to be spawned
            if (_firstPos)
            {
                return;
            }

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
        /// Build the entity from a sheet.
        /// </summary>
        public virtual bool Build(Sheet.Sheet sheet, RyzomClient client)
        {
            // Entity created.
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
