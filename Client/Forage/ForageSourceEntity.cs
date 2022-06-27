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

namespace Client.Forage
{
    /// <author>Olivier Cado</author>
    /// <author>Nevrax France</author>
    /// <date>2003</date>
    class ForageSourceEntity : Entity.Entity
    {
        /// <summary> Knowledge icon (valid only if _KnowledgeLevel > 0, otherwise NULL)</summary>
        string _IconFilename;

        /// <summary> Destination values for bars</summary>
        byte[] _BarDestValues = new byte[NbFSBarIndices];

        /// <summary> Current value for bars (except the K bar)</summary>
        float[] _BarCurrentValues = new float[NbFSBarIndices - 1];

        /// <summary> Current Display Value for bars</summary>
        byte _TimeBar;
        byte _QuantityBar;
        byte _DBar;
        byte _EBar;

        /// <summary> When an extraction is in progress, display all bars</summary>
        bool _IsExtractionInProgress;

        /// <summary> If the source is safe display its explosion bar green</summary>
        bool _SafeSource;

        /// <summary> 0=nothing 1=group 2=family. Bit 7 set means "knowledge info not received yet" (by visual fx)</summary>
        byte _KnowledgeLevel;

        /// <summary> Memorize the last received explosion bit</summary>
        byte _LastExplosionSwitch;

        /// <summary> The prospector slot (received in target)</summary>
        byte _ProspectorSlot;

        /// <summary> When the source is in "extra time", change the display of the time bar</summary>
        byte _ExtraTime;

        /// <summary> When the source is in "bonus extra time", change the display of the time bar</summary>
        byte _InclBonusExtraTime;

        /// <summary> First received value of quantity</summary>
        byte _InitialQuantity;

        /// <summary> Current quantity</summary>
        byte _CurrentQuantity;

        private const int NbFSBarIndices = 5;

        private const byte BarNotInit = 255;

        /// <summary>
        /// Constructor
        /// </summary>
        public ForageSourceEntity()
        {
            //_InSceneUserInterface= null ;
            _IconFilename = null;
            _IsExtractionInProgress = false;
            _SafeSource = false;
            _KnowledgeLevel = 0;
            _LastExplosionSwitch = 0;
            _ProspectorSlot = 255;
            _ExtraTime = 0;
            _InclBonusExtraTime = 0;
            _InitialQuantity = BarNotInit;
            _CurrentQuantity = 0;

            for (uint i = 0; i != NbFSBarIndices - 1; ++i)
            {
                _BarDestValues[i] = BarNotInit;
                _BarCurrentValues[i] = BarNotInit;
            }
            _BarDestValues[NbFSBarIndices - 1] = BarNotInit;

            // init to 0 per default (first frames...)
            _TimeBar = 0;
            _QuantityBar = 0;
            _DBar = 0;
            _EBar = 0;
        }

        /// <summary>
        /// Build the entity from a sheet.
        /// </summary>
        public override bool Build(EntitySheet sheet, RyzomClient client)
        {
            // Get FX filename and info from the sheet
            if (!(sheet is ForageSourceSheet forageSourceSheet))
            {
                client.GetLogger().Warn($"Bad sheet {(sheet != null ? sheet.Id.ToString() : "NULL")} for forage source");
                return false;
            }

            _KnowledgeLevel = forageSourceSheet.Knowledge;

            if (_KnowledgeLevel != 0)
            {
                _KnowledgeLevel |= 0x80; // we don't know the group or family yet (visual FX not received)
            }

            //// Base class init
            //Initialize();

            _type = EntityType.ForageSource;

            if (client.GetDatabaseManager() != null)
            {
                var nodeRoot = (DatabaseNodeBranch)client.GetDatabaseManager().GetNodePtr().GetNode(0);

                if (nodeRoot != null)
                {
                    _DBEntry = (DatabaseNodeBranch)nodeRoot.GetNode(_slot);

                    if (_DBEntry == null)
                    {
                        client.GetLogger().Warn("Cannot get a pointer on the DB entry.");
                    }
                }
            }

            // Set default name (with no knowledge)
            _entityName = "Raw Material Source";

            return true;
        }

        /// <summary>
        /// Update Entity Name.
        /// Interpret the property Name as the sheet id (it's not the usual string id!) of
        /// the raw material, when the knowledge is 3.
        /// </summary>
        protected override void UpdateVisualPropertyName(uint gameCycle, long prop, RyzomClient client)
        {
            //var rmSheetId = client.GetSheetIdFactory().SheetId((uint)prop);
            
            //var name = client.GetStringManager().GetItemLocalizedName(rmSheetId);

            // TODO: fix this hack
            var name = "Raw Material Source";

            if (name == "") 
                return;

            _entityName = name;

            if (_ProspectorSlot == 255) 
                return;

            var prospector = client.GetNetworkManager().GetEntityManager().GetEntity(_ProspectorSlot);

            if (prospector == null) 
                return;

            var prospectorName = prospector.GetDisplayName();

            if (!string.IsNullOrEmpty(prospectorName))
            {
                _entityName += " [" + prospectorName + "]";
            }
        }
    }
}
