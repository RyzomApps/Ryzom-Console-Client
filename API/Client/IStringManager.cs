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
    public interface IStringManager
    {
        /// <summary>
        /// request the stringId from the local cache or if missing ask the server
        /// </summary>
        bool GetString(uint stringId, out string result, INetworkManager networkManager);
    }
}