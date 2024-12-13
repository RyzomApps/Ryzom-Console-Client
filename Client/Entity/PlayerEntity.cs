///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

using API.Entity;
using Client.Database;
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
        /// <summary>
        /// 'true' while the entity is not ready to be displayed.
        /// </summary> 
        bool _WaitForAppearance;

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
            return (Mode == EntityMode.Rest || Properties.IsAfk);
        }

        /// Pointer on the Sheet with basic parameters.
        // TODO: PlayerSheet
        protected readonly Sheet.Sheet _Sheet;

        /// Pointer on the Sheet with basic parameters.
        protected RaceStatsSheet _PlayerSheet;

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

        /// <summary>
        /// Constructor
        /// </summary> 
        public PlayerEntity(RyzomClient client) : base(client)
        {
            Type = EntityType.Player;

            // Resize _Instances to the number of visual slots.
            //_Instances.resize(VisualSlot.NbSlot);

            // No sheet pointed.
            _Sheet = null;
            _PlayerSheet = null;

            // Some default colors.
            _HairColor = 0;
            _EyesColor = 0;

            // Not enough information to display the player.
            _WaitForAppearance = true;

            //_PlayerCLAsyncTextureLoading = false;

            // Light Off and not allocated
            //_LightOn = false;
        }

        /// Destructor
        //	public void Dispose();

        /// <summary>
        /// Build the entity from a sheet.
        /// </summary>
        public override bool Build(Sheet.Sheet sheet, RyzomClient client)
        {
            // Cast the sheet in the right type.
            _PlayerSheet = (RaceStatsSheet)sheet;

            if (_PlayerSheet == null)
            {
                _client.GetLogger().Error($"Player '{_slot}' sheet is not a '.race_stats' -> BIG PROBLEM.");
                return false;
            }
            else
            {
                _client.GetLogger().Debug($"Player '{_slot}' sheet is valid.");
            }

            // Get the DB Entry
            if (_client.GetDatabaseManager()?.GetServerDb() != null)
            {
                if (_client.GetDatabaseManager().GetServerDb().GetNode(0) is DatabaseNodeBranch nodeRoot)
                {
                    DbEntry = nodeRoot.GetNode(_slot) as DatabaseNodeBranch;

                    if (DbEntry == null)
                    {
                        _client.GetLogger().Error("Cannot get a pointer on the DB entry.");
                    }
                }
            }

            // Compute the first automaton.
            //_CurrentAutomaton = automatonType() + "_normal.automaton";

            // Initialize the player look.
            //init3d();
            // Compute the primitive
            //initPrimitive(0.5f, 2.0f, 0.0f, 0.0f, UMovePrimitive.DoNothing, UMovePrimitive.NotATrigger, MaskColPlayer, MaskColNone);
            // Create the collision entity (used to snap the entity to the ground).
            //computeCollisionEntity();

            // Initialize properties of the client.
            InitProperties();

            // Entity Created.
            return true;
        }

        /// <summary>
        /// Initialize properties of the entity (according to the class).
        /// </summary>
        private new void InitProperties()
        {
            Properties.IsSelectable = true;
            Properties.IsAttackable = false;
            Properties.IsGivable = true;
            Properties.IsInvitable = true;
            Properties.CanExchangeItem = true;
        }

        //private void pushInfoStr(string v)
        //{
        //    throw new NotImplementedException();
        //}

        /// Method to return the attack radius of an entity
        //	virtual double attackRadius();

        /** Return the position the attacker should have to combat according to the attack angle.
         * \param ang : 0 = the front, >0 and <Pi = left side, <0 and >-Pi = right side.
         */
        //	virtual NLMISC::CVectorD getAttackerPos(double ang, double dist);

        /// Return the People for the entity.
        //	virtual EGSPD::CPeople::TPeople people();
    }
}