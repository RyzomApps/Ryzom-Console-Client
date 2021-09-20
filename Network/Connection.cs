///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using RCC.Client;

namespace RCC.Network
{
    /// <summary>
    ///     Stores information about the connection to the ryzom server.
    /// </summary>
    public static class Connection
    {
        public static byte PlayerSelectedSlot = 0;
        public static byte ServerPeopleActive = 255;
        public static byte ServerCareerActive = 255;

        public static List<CharacterSummary> CharacterSummaries = new List<CharacterSummary>();
        public static bool WaitServerAnswer;

        public static string PlayerSelectedHomeShardName = "";
        public static string PlayerSelectedHomeShardNameWithParenthesis = "";

        public static bool GameExit = false;

        public static bool UserChar;
        public static bool NoUserChar;
        public static bool ConnectInterf;
        public static bool CreateInterf;
        public static bool CharacterInterf;

        // non ryzom variables (for workarounds)
        public static bool AutoSendCharSelection = false;
    }
}