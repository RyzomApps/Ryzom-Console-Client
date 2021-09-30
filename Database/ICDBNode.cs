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

namespace RCC.Database
{
    public abstract class ICDBNode
    {
        internal string name;

        internal CDBNodeBranch _Parent;

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
    }
}