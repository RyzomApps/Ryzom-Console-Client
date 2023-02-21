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
    /// Class to manage a player.
    /// </summary>>
    /// <author>Guillaume PUZIN</author> 
    /// <author>Nevrax France</author> 
    /// <date>2001</date> 
    public class PlayerEntity : CharacterEntity
    {
        /// Constructor
        //	CPlayerCL();

        /// Destructor
        //	public void Dispose();

        /// Build the entity from a sheet.
        //	virtual bool build(CEntitySheet sheet);

        /// Method to return the attack radius of an entity
        //	virtual double attackRadius();

        /** Return the position the attacker should have to combat according to the attack angle.
         * \param ang : 0 = the front, >0 and <Pi = left side, <0 and >-Pi = right side.
         */
        //	virtual NLMISC::CVectorD getAttackerPos(double ang, double dist);

        /// Return the People for the entity.
        //	virtual EGSPD::CPeople::TPeople people();

        /// Return a pointer on the sheet used to create this player.
        public RaceStatsSheet PlayerSheet()
        {
            return _PlayerSheet;
        }

        // Return true if this entity is a neutral entity.
        //	virtual bool isNeutral();

        // Return true if this entity is a user's friend.
        //	virtual bool isFriend();

        // Return true if this entity is a user's enemy.
        //	virtual bool isEnemy();

        // Return true if this entity is a user's ally.
        //	virtual bool isAlly();

        // Return true if this entity is neutral PVP.
        //	virtual bool isNeutralPVP();

        /// Ask if the entity is AFK (a character is never AFK but players can be)
        public virtual bool isAFK()
        {
            return (_Mode == EntityMode.Rest || EntityProperties.IsAfk);
        }

        /// Pointer on the Sheet with basic parameters.
        // TODO: PlayerSheet
        protected readonly EntitySheet _Sheet;

        /// Pointer on the Sheet with basic parameters.
        protected readonly RaceStatsSheet _PlayerSheet;

        ///// Player Face
        //protected SInstanceCL _Face = new SInstanceCL();

        /// Default Look
        protected string _DefaultChest = "";
        protected string _DefaultLegs = "";
        protected string _DefaultArms = "";
        protected string _DefaultHands = "";
        protected string _DefaultFeet = "";
        protected string _DefaultHair = "";
        protected int _HairColor = new int();
        protected int _EyesColor = new int();

        /// Update the Visual Property A
        //	virtual void updateVisualPropertyVpa(in NLMISC::TGameCycle gameCycle, in sint64 prop);

        /// Update the Visual Property B
        //	virtual void updateVisualPropertyVpb(in NLMISC::TGameCycle gameCycle, in sint64 prop);

        /// Update the Visual Property C
        //	virtual void updateVisualPropertyVpc(in NLMISC::TGameCycle gameCycle, in sint64 prop);

        /// Update the Visual Property PVP Mode (need special imp for player because of PVP consider)
        //	virtual void updateVisualPropertyPvpMode(in NLMISC::TGameCycle gameCycle, in sint64 prop);
    }
}