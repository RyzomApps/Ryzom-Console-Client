﻿///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

namespace RCC.Messages
{
    /// <summary>
    ///     A message field - TMessageFormat with message type and bit size
    /// </summary>
    internal class MessageField
    {
        byte _bitSize;
        FieldType _type;

        public MessageField(FieldType type, byte bitSize = 0)
        {
            _type = type;
            _bitSize = bitSize;
        }
    };
}