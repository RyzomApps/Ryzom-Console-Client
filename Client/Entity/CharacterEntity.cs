///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

using System.Numerics;
using API;
using API.Entity;
using Client.Client;
using Client.Database;
using Client.Sheet;

namespace Client.Entity
{
    /// <summary>
    /// Class to manage a character.
    /// </summary>
    /// <author>Guillaume PUZIN</author>
    /// <author>Nevrax France</author>
    /// <date>2001</date>
    public class CharacterEntity : Entity
    {
        /// <summary>
        /// Pointer on the Sheet with basic parameters.
        /// </summary>
        private CharacterSheet _sheet;

        private uint _eventFactionId;

        /// <summary>
        /// Is it a male ? female ? could be a beast :p
        /// </summary>
        private EntityGender _gender;

        private uint _guildNameId;
        private ulong _guildSymbol;
        private uint _leagueId;

        /// <summary>
        /// The mode the entity should already be but still not the current one.
        /// </summary>
        private EntityMode _modeWanted = new EntityMode();

        private ushort _outpostId;
        private readonly PvpSide _outpostSide = new PvpSide();
        private PvpClan _pvpClan = new PvpClan();
        private ushort _pvpMode;

        /// <summary>
        /// Can the user select this entity by pressing space key?
        /// </summary> 
        private bool _selectableBySpace;

        /// <summary>
        /// 0 -> male
        /// 1 -> female
        /// 2 -> beast
        /// </summary>
        private EntityGender GetGender()
        {
            return _gender;
        }

        private void SetGender(EntityGender gender)
        {
            _gender = gender;
        }

        /// <summary>
        /// Return the sheet of the character
        /// </summary>
        private CharacterSheet GetSheet()
        {
            return _sheet;
        }

        /// <summary>
        /// Return true if this entity is a Kami
        /// </summary>
        public bool IsKami()
        {
            //if (_sheet == null)
            //{
            return false;
            //}

            //return (_sheet.Race == EGSPD.CPeople.Kami);
        }

        /// <summary>
        /// Return true if this entity has Race set to Unknown
        /// </summary>
        public bool IsUnknownRace()
        {
            //if (_sheet == null)
            //{
            return false;
            //}

            //return (_sheet.Race == EGSPD.CPeople.Unknown);
        }

        /// <summary>
        /// Return true if the character is swimming (even if dead).
        /// </summary>
        protected bool IsSwimming()
        {
            return Mode == EntityMode.Swim || Mode == EntityMode.SwimDeath || Mode == EntityMode.MountSwim;
        }

        /// <summary>
        /// Return true if the character is riding.
        /// </summary>
        protected bool IsRiding()
        {
            return Mode == EntityMode.MountNormal || Mode == EntityMode.MountSwim;
        }

        /// <summary>
        /// Is the entity in combat.
        /// </summary>
        protected bool IsFighting()
        {
            return Mode == EntityMode.Combat || Mode == EntityMode.CombatFloat;
        }

        /// <summary>
        /// Return true if the character is currently dead.
        /// </summary>
        protected new bool IsDead()
        {
            return Mode == EntityMode.Death || Mode == EntityMode.SwimDeath;
        }

        /// <summary>
        /// Return true if the character is really dead. With no lag because of anim or LCT
        /// </summary>
        protected bool IsReallyDead()
        {
            return TheoreticalMode == EntityMode.Death || TheoreticalMode == EntityMode.SwimDeath;
        }

        /// <summary>
        /// Return 'true' is the entity is displayed.
        /// </summary>
        protected bool IsVisible()
        {
            return true;
        }

        protected uint GetGuildNameID()
        {
            return _guildNameId;
        }

        protected ulong GetGuildSymbol()
        {
            return _guildSymbol;
        }

        protected uint GetEventFactionID()
        {
            return _eventFactionId;
        }

        protected ushort GetPvpMode()
        {
            return _pvpMode;
        }

        protected void SetPvpMode(ushort mode)
        {
            _pvpMode = mode;
        }

        protected uint GetLeagueID()
        {
            return _leagueId;
        }

        protected void SetLeagueID(uint league)
        {
            _leagueId = league;
        }

        protected ushort GetOutpostId()
        {
            return _outpostId;
        }

        protected PvpSide GetOutpostSide()
        {
            return _outpostSide;
        }

        /// <summary>
        /// Ask if the entity is sitting
        /// </summary>
        protected bool IsSit()
        {
            return Mode == EntityMode.Sit;
        }


        /// <summary>
        /// Build the entity from a sheet.
        /// </summary>
        public override bool Build(EntitySheet sheet, RyzomClient client)
        {
            // Cast the sheet in the right type.
            _sheet = sheet as CharacterSheet;

            if (_sheet == null)
            {
                client.GetLogger().Warn("This is not a character sheet -> entity not initialized.");
                return false;
            }

            // Type
            Type = _sheet.Race >= PeopleType.Creature ? EntityType.Fauna : EntityType.NPC;

            // Names
            if (Type == EntityType.Fauna)
            {
                // Get the fauna name in the sheet
                var creatureName = client.GetStringManager().GetCreatureLocalizedName(_sheet.Id);

                if (!creatureName.StartsWith("<NotExist:"))
                {
                    _entityName = creatureName;
                }
            }

            // Get the DB Entry
            if (client.GetDatabaseManager().GetNodePtr() != null)
            {
                if (client.GetDatabaseManager().GetNodePtr().GetNode(0) is DatabaseNodeBranch nodeRoot)
                {
                    DbEntry = nodeRoot.GetNode(_slot) as DatabaseNodeBranch;

                    if (DbEntry == null)
                    {
                        client.GetLogger().Warn("Cannot get a pointer on the DB entry.");
                    }
                }
            }

            // Get the Character gender.
            _gender = (EntityGender)_sheet.Gender;

            // Initialize properties of the entity (selectable/attackable/etc.).
            InitProperties();

            // Entity created.
            return true;
        }

        /// <summary>
        /// Initialize properties of the entity (according to the class).
        /// </summary>
        public void InitProperties()
        {
            Properties.IsSelectable = _sheet.Selectable;
            Properties.IsTalkableTo = _sheet.Talkable;
            Properties.IsAttackable = _sheet.Attackable;
            Properties.IsGivable = _sheet.Givable;
            Properties.IsMountable = _sheet.Mountable;
            Properties.IsInvitable = false; // You cannot group with a bot.
            Properties.IsAfk = false;

            // TODO: Implement HLState
            //switch (_sheet.HLState)
            //{
            //    case LHSTATE.LOOTABLE:
            //        EntityProperties.IsLootable(true); // You can loot the creature
            //        EntityProperties.IsHarvestable(false); // You cannot harvest the creature
            //        break;
            //    case LHSTATE.HARVESTABLE:
            //        EntityProperties.IsLootable(false); // You cannot loot the creature
            //        EntityProperties.IsHarvestable(true); // You can harvest the creature
            //        break;
            //    case LHSTATE.LOOTABLE_HARVESTABLE:
            //        EntityProperties.IsLootable(true); // You can loot the creature
            //        EntityProperties.IsHarvestable(true); // You can harvest the creature
            //        break;
            //
            //    default:
            //        EntityProperties.IsLootable(false); // You cannot loot the creature
            //        EntityProperties.IsHarvestable(false); // You cannot harvest the creature
            //        break;
            //}
        }

        private const int PropertyPosX = 0;
        private const int PropertyPosY = 1;
        private const int PropertyPosZ = 2;
        private const int PropertyOrientation = 3; // Theta

        /// <summary>
        /// New mode received.
        /// </summary>
        /// <remarks>For the first mode, we must have received the position and orientation (but this should be the case).<br/>
        /// Read the position or orientation from the database when reading the mode (no more updated in updateVisualPropertyPos and updateVisualPropertyOrient).</remarks>
        protected override void UpdateVisualPropertyMode(uint gameCycle, long prop, IClient client)
        {
            client.GetLogger().Debug($"({client.GetApiNetworkManager().GetCurrentServerTick()}) CH:updtVPMode:{_slot}: '{(EntityMode)prop}({prop})' received.");

            // New Mode Received : Set the Theoretical Current Mode if different.
            if (TheoreticalMode != (EntityMode)(prop & 0xffff))
            {
                TheoreticalMode = (EntityMode)(prop & 0xffff);
            }
            else
            {
                client.GetLogger().Warn($"CH:updtVPMode:{_slot}: The mode '{TheoreticalMode}({(int)TheoreticalMode})' sent is the same as the current one.");
                return;
            }

            // If it is the first mode, set the mode.
            if (Mode == EntityMode.UnknownMode)
            {
                // SET THE FIRST POSITION
                //-----------------------
                // Check the DB entry (the warning is already done in the build method).
                if (DbEntry == null)
                {
                    return;
                }

                // Get The property 'PROPERTY_POSX'.
                if (!(DbEntry.GetNode(PropertyPosX) is DatabaseNodeLeaf nodeX))
                {
                    client.GetLogger().Warn($"CH::updtVPMode:{_slot}: Cannot find the property 'PROPERTY_POSX({PropertyPosX})'.");
                    return;
                }

                // Get The property 'PROPERTY_POSY'.
                if (!(DbEntry.GetNode(PropertyPosY) is DatabaseNodeLeaf nodeY))
                {
                    client.GetLogger().Warn($"CH::updtVPMode:{_slot}: Cannot find the property 'PROPERTY_POSY({PropertyPosY})'.");
                    return;
                }

                // Get The property 'PROPERTY_POSZ'.
                if (!(DbEntry.GetNode(PropertyPosZ) is DatabaseNodeLeaf nodeZ))
                {
                    client.GetLogger().Warn($"CH::updtVPMode:{_slot}: Cannot find the property 'PROPERTY_POSZ({PropertyPosZ})'.");
                    return;
                }

                // Next position will no longer be the first one.
                _firstPos = false;

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
                if (!(DbEntry.GetNode(PropertyOrientation) is DatabaseNodeLeaf nodeOri))
                {
                    client.GetLogger().Warn($"CH::updtVPMode:{_slot}: Cannot find the property 'PROPERTY_ORIENTATION({PropertyOrientation})'.");
                    return;
                }

                //C64BitsParts parts = new C64BitsParts();
                //parts.i64[0] = nodeOri.GetValue64();
                //float angleZ = parts.f[0];

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
                Mode = TheoreticalMode;
                //_ModeWanted = _TheoreticalMode;

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
                Mode = TheoreticalMode;

                // Float mode push the orientation
                if (TheoreticalMode == EntityMode.CombatFloat)
                {
                    // Get The property 'PROPERTY_ORIENTATION'.

                    if (!(DbEntry.GetNode(PropertyOrientation) is DatabaseNodeLeaf nodeOri))
                    {
                        client.GetLogger().Warn($"CH::updtVPMode:{_slot}: Cannot find the property 'PROPERTY_ORIENTATION({PropertyOrientation})'.");
                        return;
                    }

                    //_Stages.addStage(gameCycle, PROPERTY_ORIENTATION, nodeOri.GetValue64());
                }
                // Any other mode push the position
                else
                {
                    if (TheoreticalMode != EntityMode.MountNormal)
                    {
                        // Check the DB entry (the warning is already done in the build method).
                        if (DbEntry == null)
                        {
                            return;
                        }

                        // Get The property 'PROPERTY_POSX'.
                        if (!(DbEntry.GetNode(PropertyPosX) is DatabaseNodeLeaf nodeX))
                        {
                            client.GetLogger().Warn("CH::updtVPMode:%d: Cannot find the property 'PROPERTY_POSX(%d)'.", _slot, PropertyPosX);
                            return;
                        }

                        // Get The property 'PROPERTY_POSY'.
                        if (!(DbEntry.GetNode(PropertyPosY) is DatabaseNodeLeaf nodeY))
                        {
                            client.GetLogger().Warn("CH::updtVPMode:%d: Cannot find the property 'PROPERTY_POSY(%d)'.", _slot, PropertyPosY);
                            return;
                        }

                        // Get The property 'PROPERTY_POSZ'.
                        if (!(DbEntry.GetNode(PropertyPosZ) is DatabaseNodeLeaf nodeZ))
                        {
                            client.GetLogger().Warn("CH::updtVPMode:%d: Cannot find the property 'PROPERTY_POSZ(%d)'.", _slot, PropertyPosZ);
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
    }
}