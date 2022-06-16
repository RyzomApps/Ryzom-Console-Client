///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

namespace Client.Client
{
    /// <summary>
    /// Implement this class if you want to wait for
    /// string to be delivered.
    /// </summary>
    public abstract class StringWaitCallback
    {
        /// Overide this method to receive callback for dynamic string.
        public abstract void OnDynStringAvailable(uint stringId, string value);
    };
}