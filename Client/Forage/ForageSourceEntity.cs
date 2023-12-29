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
    internal class ForageSourceEntity : Entity.Entity, IForageSourceEntity
    {
        private const byte BarNotInit = 255;

        /// <summary>
        /// Destination values for bars
        /// </summary>
        private readonly byte[] _barDestValues = new byte[(int)TfsBarIndex.NbFsBarIndices];

        /// <summary>
        /// Current value for bars (except the K bar)
        /// </summary>
        private readonly float[] _barCurrentValues = new float[(int)TfsBarIndex.NbFsBarIndices - 1];

        /// <summary>
        /// Current Display Value for bars
        /// </summary>
        private byte _timeBar;
        private byte _quantityBar;
        private byte _dBar;
        private byte _eBar;

        /// <summary>
        /// When an extraction is in progress, display all bars
        /// </summary>
        private bool _isExtractionInProgress;

        /// <summary>
        /// If the source is safe display its explosion bar green
        /// </summary>
        private bool _safeSource;

        /// <summary>
        /// 0=nothing 1=group 2=family. Bit 7 set means "knowledge info not received yet" (by visual fx)
        /// </summary>
        private byte _knowledgeLevel;

        /// <summary>
        /// Memorize the last received explosion bit
        /// </summary>
        private byte _lastExplosionSwitch;

        /// <summary>
        /// The prospector slot (received in target)
        /// </summary>
        private byte _prospectorSlot;

        /// <summary>
        /// When the source is in "extra time", change the display of the time bar
        /// </summary>
        private byte _extraTime;

        /// <summary>
        /// When the source is in "bonus extra time", change the display of the time bar
        /// </summary>
        private byte _inclBonusExtraTime;

        /// <summary>
        /// First received value of quantity
        /// </summary>
        private byte _initialQuantity;

        /// <summary>
        /// Current quantity
        /// </summary>
        private byte _currentQuantity;

        /// <summary>
        /// Constructor
        /// </summary>
        public ForageSourceEntity(RyzomClient client) : base(client)
        {
            _isExtractionInProgress = false;
            _safeSource = false;
            _knowledgeLevel = 0;
            _lastExplosionSwitch = 0;
            _prospectorSlot = 255;
            _extraTime = 0;
            _inclBonusExtraTime = 0;
            _initialQuantity = BarNotInit;
            _currentQuantity = 0;

            for (uint i = 0; i != (int)TfsBarIndex.NbFsBarIndices - 1; ++i)
            {
                _barDestValues[i] = BarNotInit;
                _barCurrentValues[i] = BarNotInit;
            }

            _barDestValues[(int)TfsBarIndex.NbFsBarIndices - 1] = BarNotInit;

            // init to 0 per default (first frames...)
            _timeBar = 0;
            _quantityBar = 0;
            _dBar = 0;
            _eBar = 0;
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

            _knowledgeLevel = forageSourceSheet.Knowledge;

            if (_knowledgeLevel != 0)
            {
                _knowledgeLevel |= 0x80; // we don't know the group or family yet (visual FX not received)
            }

            //// Base class init
            //Initialize();

            Type = EntityType.ForageSource;

            if (client.GetDatabaseManager() != null)
            {
                var nodeRoot = (DatabaseNodeBranch)client.GetDatabaseManager().GetNodePtr().GetNode(0);

                if (nodeRoot != null)
                {
                    DbEntry = (DatabaseNodeBranch)nodeRoot.GetNode(_slot);

                    if (DbEntry == null)
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
        protected override void UpdateVisualPropertyName(uint _, long prop, RyzomClient client)
        {
            _entityName = "Raw Material Source";

            if (_prospectorSlot == Constants.InvalidSlot)
                return;

            var prospector = client.GetNetworkManager().GetEntityManager().GetEntity(_prospectorSlot);

            if (prospector == null)
                return;

            var prospectorName = prospector.GetDisplayName();

            if (!string.IsNullOrEmpty(prospectorName))
            {
                _entityName += $" [{prospectorName}]";
            }
        }

        /// <summary>
        /// Update Entity Target.
        /// </summary>
        protected override void UpdateVisualPropertyTarget(uint _, long prop, RyzomClient client)
        {
            var slot = (byte)prop;

            if (slot == Constants.InvalidSlot) return;

            _prospectorSlot = slot;

            // NULL if entity not received
            var prospector = client.GetNetworkManager().GetEntityManager().GetEntity(_prospectorSlot);

            if (prospector == null) return;
            var prospectorName = prospector.GetDisplayName();

            if (!string.IsNullOrEmpty(prospectorName))
            {
                _entityName += $" [{prospectorName}]";
            }
        }

        /// <summary>
        /// Update Entity Bars
        /// </summary>
        protected override void UpdateVisualPropertyBars(uint _, long prop, RyzomClient client)
        {
            // NB: forage don't use CBarManager for 2 reasons: useless (forage bars exist only through VP),
            // and complicated since updated at each frame on client (because of smooth transition code below)
            var setBarsNow = _barDestValues[0] == BarNotInit;

            // Time to live
            _barDestValues[(int)TfsBarIndex.FsbTime] = (byte)(prop & 0x7f);

            // D
            _barDestValues[(int)TfsBarIndex.FsbD] = (byte)((prop >> 14) & 0x7f);

            // E
            _barDestValues[(int)TfsBarIndex.FsbE] = (byte)((prop >> 21) & 0x7f);

            // Deal with safe sources
            if (_barDestValues[(int)TfsBarIndex.FsbE] == 0)
            {
                // Safe source
                _barDestValues[(int)TfsBarIndex.FsbE] = 127;
                _safeSource = true;
            }
            else
            {
                _safeSource = false;
            }

            if (setBarsNow)
            {
                SetBarValue(out _barCurrentValues[(int)TfsBarIndex.FsbTime], out _timeBar, _barDestValues[(int)TfsBarIndex.FsbTime]);

                _initialQuantity = ((byte)((prop >> 7) & 0x7f)); // Quantity

                if (_initialQuantity != 0)
                {
                    _currentQuantity = _initialQuantity;
                    _barDestValues[(int)TfsBarIndex.FsbQuantiy] = 127;
                }
                else
                {
                    _currentQuantity = 0;
                    _barDestValues[(int)TfsBarIndex.FsbQuantiy] = 0;
                }

                SetBarValue(out _barCurrentValues[(int)TfsBarIndex.FsbQuantiy], out _quantityBar, _barDestValues[(int)TfsBarIndex.FsbQuantiy]);
                SetBarValue(out _barCurrentValues[(int)TfsBarIndex.FsbD], out _dBar, _barDestValues[(int)TfsBarIndex.FsbD]);
                SetBarValue(out _barCurrentValues[(int)TfsBarIndex.FsbE], out _eBar, _barDestValues[(int)TfsBarIndex.FsbE]);
            }
            else
            {
                _currentQuantity = (byte)((prop >> 7) & 0x7f);

                // Quantity
                _barDestValues[(int)TfsBarIndex.FsbK] = (byte)(_initialQuantity != 0 ? _currentQuantity * 127 / _initialQuantity : 0);
            }
        }

        /// <summary>
        /// Update Entity Orientation.
        /// Used to carry the kami anger bar (does not change often)
        /// </summary>
        protected override void UpdateVisualPropertyOrient(uint _, long prop, RyzomClient client)
        {
            var u = (uint)prop;
            _barDestValues[(int)TfsBarIndex.FsbQuantiy] = (byte)(u & 0x7f);
            _extraTime = (byte)((u >> 7) & 0x7f);
            _inclBonusExtraTime = (byte)((u >> 14) & 0x7f);
        }

        /// <summary>
        /// Update Visual FX.
        /// Contains group or family (if knowledge is 1 or 2-3), and explosion state.
        /// </summary>
        protected override void UpdateVisualPropertyVisualFX(uint _, long prop, RyzomClient client)
        {
            // Display explosion FX if the switch flag tells us to do it
            _lastExplosionSwitch = (byte)((prop & 0x400) >> 10);

            // Set family or group knowledge info
            if ((_knowledgeLevel & 0x80) == 0)
                return;

            // 10 bits
            var index = (uint)(prop & 0x3ff);
            _knowledgeLevel &= 0x7F;

            _entityName = _knowledgeLevel switch
            {
                1 => $"Raw Material Source (Group {index})",
                2 => $"Raw Material Source (Family {index})",
                _ => _entityName
            };

            if (_knowledgeLevel > 2 || _prospectorSlot == 255)
                return;

            // NULL if entity not received
            var prospector = client.GetNetworkManager().GetEntityManager().GetEntity(_prospectorSlot);

            if (prospector == null)
                return;

            var prospectorName = prospector.GetDisplayName();

            if (!string.IsNullOrEmpty(prospectorName))
            {
                _entityName += $" [{prospectorName}]";
            }
        }

        /// <summary>
        /// Helper for updateVisualPropertyBars()
        /// </summary>
        private static void SetBarValue(out float currentValue, out byte displayedValue, byte newValue)
        {
            displayedValue = newValue;
            currentValue = displayedValue;
        }

        /// <inheritdoc />
        public byte GetCurrentBarValue(TfsBarIndex index)
        {
            return 0 <= index && index < TfsBarIndex.NbFsBarIndices ? _barDestValues[(int)index] : byte.MaxValue;
        }
    }
}
