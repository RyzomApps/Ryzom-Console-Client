///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

namespace Client.Network
{
    /// <summary>
    /// Define the online state of a character 
    /// </summary>
    public enum CharConnectionState : byte
    {
        /// <summary>
        /// the character is offline
        /// </summary>
        CcsOffline = 0,
        /// <summary>
        /// the character is online on the same shard
        /// </summary>
        CcsOnline = 1,
        /// <summary>
        /// the character is online, but on another shard in the domain.
        /// </summary>
        CcsOnlineAbroad = 2
    };
}