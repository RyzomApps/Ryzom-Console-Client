///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

namespace API.Network
{
    /// <summary>
    /// used to control the connection
    /// </summary>
    public interface INetworkManager
    {
        string PlayerSelectedHomeShardName { get; set; }

        /// <summary>
        /// Send - updates when packets were received
        /// </summary>
        void Send(uint gameCycle);

        /// <summary>
        /// Updates the whole connection with the frontend.
        /// Call this method evently.
        /// </summary>
        /// <returns>'true' if data were sent/received.</returns>
        bool Update();

        /// <summary>
        /// Send updates
        /// </summary>
        void Send();

        /// <summary>
        /// sendMsgToServer Helper
        /// selects the message by its name and pushes it to the connection
        /// </summary>
        void SendMsgToServer(string sMsg);

        /// <summary>
        /// Buffers a target action
        /// </summary>
        void PushTarget(in byte slot);
    }
}
