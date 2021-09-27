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
        public override void SetParent(CDBNodeBranch parent)
        {

        }

        public override void SetAtomic(bool atomBranch)
        {

        }

        internal override void Init(XmlElement child, Action progressCallBack)
        {

        }
    }
}