///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

namespace RCC.Network
{
    /// <summary>
    ///     system messages used in the connection state machine
    /// </summary>
    internal enum SystemMessageType : byte
    {
        /// <summary>
        ///     From client
        /// </summary>
        SystemLoginCode = 0,

        /// <summary>
        ///     From server
        /// </summary>
        SystemSyncCode = 1,

        /// <summary>
        ///     From client
        /// </summary>
        SystemAckSyncCode = 2,

        /// <summary>
        ///     From server
        /// </summary>
        SystemProbeCode = 3,

        /// <summary>
        ///     From client
        /// </summary>
        SystemAckProbeCode = 4,

        /// <summary>
        ///     From client
        /// </summary>
        SystemDisconnectionCode = 5,

        /// <summary>
        ///     From server
        /// </summary>
        SystemStalledCode = 6,

        /// <summary>
        ///     From server
        /// </summary>
        SystemServerDownCode = 7,

        /// <summary>
        ///     From client
        /// </summary>
        SystemQuitCode = 8,

        /// <summary>
        ///     From server
        /// </summary>
        SystemAckQuitCode = 9
    }
}