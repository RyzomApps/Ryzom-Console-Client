///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

namespace RCC.Database
{
    public abstract class BranchObserverBase
    {
        ~BranchObserverBase() { }

        public virtual DatabaseNodeBase Owner() { return null; }

        public virtual bool ObservesLeaf(string leafName) { return false; }

        public virtual bool InList(uint list) { return false; }

        public virtual void AddToFlushableList() { }

        public virtual void RemoveFromFlushableList(uint list) { }

        public virtual void RemoveFromFlushableList() { }
    }
}
