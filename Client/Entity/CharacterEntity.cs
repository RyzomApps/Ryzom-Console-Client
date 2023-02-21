///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

using API.Entity;
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
        private readonly EntitySheet _sheet;

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
        private EntitySheet GetSheet()
        {
            return _sheet;
        }

        /// <summary>
        /// Return true if this entity is a Kami
        /// </summary>
        public bool IsKami()
        {
            //if (_Sheet == null)
            //{
                return false;
            //}

            //return (_Sheet.Race == EGSPD.CPeople.Kami);
        }

        /// <summary>
        /// Return true if this entity has Race set to Unknown
        /// </summary>
        public bool IsUnknownRace()
        {
            //if (_Sheet == null)
            //{
                return false;
            //}

            //return (_Sheet.Race == EGSPD.CPeople.Unknown);
        }

        /// <summary>
        /// Return true if the character is swimming (even if dead).
        /// </summary>
        protected bool IsSwimming()
        {
            return _Mode == EntityMode.Swim || _Mode == EntityMode.SwimDeath || _Mode == EntityMode.MountSwim;
        }

        /// <summary>
        /// Return true if the character is riding.
        /// </summary>
        protected bool IsRiding()
        {
            return _Mode == EntityMode.MountNormal || _Mode == EntityMode.MountSwim;
        }

        /// <summary>
        /// Is the entity in combat.
        /// </summary>
        protected bool IsFighting()
        {
            return _Mode == EntityMode.Combat || _Mode == EntityMode.CombatFloat;
        }

        /// <summary>
        /// Return true if the character is currently dead.
        /// </summary>
        protected bool IsDead()
        {
            return _Mode == EntityMode.Death || _Mode == EntityMode.SwimDeath;
        }

        /// <summary>
        /// Return true if the character is really dead. With no lag because of anim or LCT
        /// </summary>
        protected bool IsReallyDead()
        {
            return _TheoreticalMode == EntityMode.Death || _TheoreticalMode == EntityMode.SwimDeath;
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
            return _Mode == EntityMode.Sit;
        }
    }
}