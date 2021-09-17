// This code is a modified version of a file from the 'Ryzom - MMORPG Framework'
// <http://dev.ryzom.com/projects/ryzom/>,
// which is released under GNU Affero General Public License.
// <http://www.gnu.org/licenses/>
// Original Copyright 2010 by Winch Gate Property Limited

namespace RCC.NetworkAction
{
    public abstract class ActionImpulsion : Action
    {
        public bool AllowExceedingMaxSize;

        public override void reset()
        {
            AllowExceedingMaxSize = false;
        }
    }
}