///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

namespace Client.Messages
{
    /// <summary>
    /// type of the variable used in MessageField of the MessageNode
    /// </summary>
    public enum MessageFieldType
    {
        Bool,
        Sint8,
        Sint16,
        Sint32,
        Sint64,
        Uint8,
        Uint16,
        Uint32,
        Uint64,
        BitSizedUint,
        Float,
        Double,
        EntityId,
        String,
        UcString
    };
}