///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

namespace RCC.NetworkAction
{
    /// <summary>
    ///     base for actions with impusions
    /// </summary>
    public abstract class ActionImpulsion : Action
    {
        /// <summary>
        ///     allow exceeding the maximum size of the message
        /// </summary>
        public bool AllowExceedingMaxSize;

        /// <summary>
        ///     reset the action
        /// </summary>
        public override void Reset()
        {
            AllowExceedingMaxSize = false;
        }
    }
}