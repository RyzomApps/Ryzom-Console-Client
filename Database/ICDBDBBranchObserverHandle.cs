///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

namespace RCC.Database
{
    public abstract class ICDBDBBranchObserverHandle
    {
        ~ICDBDBBranchObserverHandle() { }

        public virtual ICDBNode owner() { return null; }
        //public virtual IPropertyObserver observer() { }
        public virtual bool observesLeaf(string leafName) { return false; }
        public virtual bool inList(uint list) { return false; }
        public virtual void addToFlushableList() { }
        public virtual void removeFromFlushableList(uint list) { }
        public virtual void removeFromFlushableList() { }

    }
}
