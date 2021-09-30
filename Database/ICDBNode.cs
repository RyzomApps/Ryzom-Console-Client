///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

using System;
using System.Data;
using System.Xml;
using RCC.Network;

namespace RCC.Database
{
    public abstract class ICDBNode
    {
        internal string name;

        internal CDBNodeBranch _Parent;

        /// Atomic flag: is the branch an atomic group, or is the leaf a member of an atomic group
        internal bool _AtomicFlag;

        internal virtual void SetParent(CDBNodeBranch parent)
        {
            _Parent = parent;
        }

        internal abstract void SetAtomic(bool atomBranch);

        internal abstract void Init(XmlElement child, Action progressCallBack);

        /// <summary>
        /// get the parent of a node
        /// </summary>
        internal virtual CDBNodeBranch getParent()
        {
            return _Parent;
        }

        /// <summary>
        /// get the name of this node
        /// </summary>
        internal virtual string getName()
        {
            return name;
        }

        internal abstract void readDelta(uint gc, BitMemoryStream f);

        internal abstract CDBNodeLeaf findLeafAtCount(uint count);

        /// Set the atomic branch flag (when all the modified nodes of a branch should be tranmitted at the same time)
        internal virtual void setAtomic(bool atomicBranch) { _AtomicFlag = atomicBranch; }

        /// Return true if the branch has the atomic flag
        internal virtual bool isAtomic() { return _AtomicFlag; }
    }
}