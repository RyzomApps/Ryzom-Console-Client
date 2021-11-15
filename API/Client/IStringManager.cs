///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

using API.Network;

namespace API.Client
{
    /// <summary>
    /// Manage string organized as conditional clause grouped into phrase.
    /// This class can choose at runtime one of the clause depending
    /// on passed parameters.
    /// </summary>
    public interface IStringManager
    {
        /// <summary>
        /// request the stringId from the local cache or if missing ask the server
        /// </summary>
        bool GetString(uint stringId, out string result, INetworkManager networkManager);
    }
}