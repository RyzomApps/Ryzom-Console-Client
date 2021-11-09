///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

namespace RCC.Network.Action
{
    /// <summary>
    /// type of the action or action property
    /// </summary>
    public enum ActionCode : byte
    {
        ActionPositionCode = 0,
        ActionGenericCode = 1,
        ActionGenericMultiPartCode = 2,
        ActionSint64 = 3,

        ActionSyncCode = 10,
        ActionDisconnectionCode = 11,
        ActionAssociationCode = 12,
        ActionLoginCode = 13,

        ActionTargetSlotCode = 40,

        ActionDummyCode = 99
    }
}