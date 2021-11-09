///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

namespace Client.Property
{
    /// <summary>
    /// A property change
    /// </summary>
    public class Change
    {
        /// <summary>
        /// Constructor
        /// </summary>
        internal Change(byte id = 0, byte prop = 255, uint gc = 0)
        {
            ShortId = id;
            Property = prop;
            GameCycle = gc;
        }

        /// <summary>
        /// Slot
        /// </summary>
        internal byte ShortId;

        /// <summary>
        /// Property index
        /// </summary>
        internal byte Property;

        /// <summary>
        /// The tick when the property changed, in server time
        /// </summary>
        internal uint GameCycle;

        #region These contextual additional properties must be set by hand (not by constructor)

        /// <summary>
        /// Position additional information (prediction & interior)
        /// </summary>
        internal TPositionInfo PositionInfo;

        /// <summary>
        /// Additional information when creating an entitys
        /// </summary>
        internal TNewEntityInfo NewEntityInfo;

        #endregion

        /// <summary>
        /// Additional info for position updates
        /// </summary>
        internal struct TPositionInfo
        {
            /// <summary>
            /// Only for positions (property index 0): interval, in game cycle unit, between the current
            /// position update and the next predicted one. In most cases the next real position update
            /// will occur just before the predicted interval elapses.
            /// 
            /// If the interval cannot be predicted yet (e.g. before having received two updates
            /// for the same entity), PredictedInterval is set to ~0.
            /// </summary>
            internal uint PredictedInterval;

            /// <summary>
            /// Is position interior (only for position)
            /// </summary>
            internal bool IsInterior;
        }

        public struct TNewEntityInfo
        {
            internal void Reset()
            {
                DataSetIndex = 2 ^ 20 - 1;
                Alias = 0;
            }

            /// <summary>
            /// Compressed dataset row
            /// </summary>
            internal uint DataSetIndex;

            /// <summary>
            /// Alias (only for mission giver NPCs)
            /// </summary>
            internal uint Alias;
        };
    }
}
