///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

using API.Helper;
using Client.Network;
using Client.Stream;

namespace Client.Messages
{
    /// <summary>
    /// decode server client message for the character information at the beginning
    /// </summary>
    /// <author>PUZIN Guillaume (GUIGUI)</author>
    /// <author>Nevrax France</author>
    /// <date>2002</date>
    public static class UserCharMsg
    {
        /// <summary>
        /// Decode the message
        /// </summary>
        internal static void Read(BitMemoryStream impulse, out int x, out int y, out int z, out float heading, out short season, out int userRole, out bool isInRingSession, out int highestMainlandSessionId, out int firstConnectedTime, out int playedTime)
        {
            x = 0;
            y = 0;
            z = 0;

            var headingI = 0;

            var s = impulse;
            var f = s;

            f.Serial(ref x);
            f.Serial(ref y);
            f.Serial(ref z);
            f.Serial(ref headingI);

            heading = Misc.Int32BitsToSingle(headingI);

            short v = 0;
            s.Serial(ref v, 3);
            season = v;
            v = 0;
            s.Serial(ref v, 3);
            userRole = v & 0x3;
            isInRingSession = (v & 0x4) != 0;

            highestMainlandSessionId = 0;
            firstConnectedTime = 0;
            playedTime = 0;

            s.Serial(ref highestMainlandSessionId);
            s.Serial(ref firstConnectedTime);
            s.Serial(ref playedTime);
        }
    }
}
