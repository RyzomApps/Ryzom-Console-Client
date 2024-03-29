﻿///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

using System;

namespace Client.Inventory
{
    public interface InventoryCategoryTemplate
    {
        uint NbInventoryIds { get; }

        uint[] InventoryNbSlots { get; }
        string[] InventoryStr { get; }
        uint SlotBitSize { get; }

        string GetDbStr(uint invId);

        bool NeedPlainInfoVersionTransfer();
    }
}