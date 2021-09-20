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
    /// Define the online state of a character 
    /// </summary>
    public enum CharConnectionState : byte
    {
        /// the character is offline
        CcsOffline = 0,
        /// the character is online on the same shard
        CcsOnline = 1,
        /// the character is online, but on another shard in the domain.
        CcsOnlineAbroad = 2
    };
}