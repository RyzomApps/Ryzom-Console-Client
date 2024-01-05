///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

namespace Client.Strings
{
    /// <summary>
    /// Implement this class if you want to wait for
    /// string to be delivered.
    /// </summary>
    public abstract class StringWaitCallback
    {
        /// <summary>Receive callback for dynamic string.</summary>
        public abstract void OnDynStringAvailable(uint dynStringId, string value);

        /// <summary>Receive callback for string.</summary>
        public abstract void OnStringAvailable(uint stringId, in string value);
    }
}