﻿// This code is a modified version of a file from the 'Ryzom - MMORPG Framework'
// <http://dev.ryzom.com/projects/ryzom/>,
// which is released under GNU Affero General Public License.
// <http://www.gnu.org/licenses/>
// Original Copyright 2010 by Winch Gate Property Limited

namespace RCC.Network
{
    internal enum SystemMessage : byte
    {
        /// <summary>
        /// From client
        /// </summary>
        SystemLoginCode = 0,
        /// <summary>
        /// From server
        /// </summary>
        SystemSyncCode = 1,
        /// <summary>
        /// From client
        /// </summary>
        SystemAckSyncCode = 2,
        /// <summary>
        /// From server
        /// </summary>
        SystemProbeCode = 3,
        /// <summary>
        /// From client
        /// </summary>
        SystemAckProbeCode = 4,
        /// <summary>
        /// From client
        /// </summary>
        SystemDisconnectionCode = 5,
        /// <summary>
        /// From server
        /// </summary>
        SystemStalledCode = 6,
        /// <summary>
        /// From server
        /// </summary>
        SystemServerDownCode = 7,
        /// <summary>
        /// From client
        /// </summary>
        SystemQuitCode = 8,
        /// <summary>
        /// From server
        /// </summary>
        SystemAckQuitCode = 9,
    }
}