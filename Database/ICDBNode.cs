﻿///////////////////////////////////////////////////////////////////
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
    public abstract class ICDBNode
    {
        public string name;
        public abstract void SetParent(CDBNodeBranch parent);
        public abstract void SetAtomic(bool atomBranch);
        internal abstract void Init(XmlElement child, Action progressCallBack);
    }
}