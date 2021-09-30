///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

using System;
using System.Xml;

namespace RCC.Database
{
    public class CDBNodeLeaf : ICDBNode
    {
        public CDBNodeLeaf(string name)
        {
            this.name = name;
        }

        internal override void SetAtomic(bool atomBranch)
        {

        }

        internal override void Init(XmlElement child, Action progressCallBack)
        {

        }
    }
}