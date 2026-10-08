///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
using API;
using API.Commands;
using Client.Stream;
using System;
using System.Collections.Generic;
using System.Text;

namespace Client.Commands
{
    /// <summary>
    /// Formats raw money values (dappers) with Ryzom-style space separators.
    /// </summary>
    public static class MoneyFormat
    {
        public static string Format(long value)
        {
            var sign = value < 0 ? "-" : "";
            var digits = (value < 0 ? -value : value).ToString(System.Globalization.CultureInfo.InvariantCulture);
            var result = new StringBuilder();
            for (var i = 0; i < digits.Length; i++)
            {
                if (i > 0 && (digits.Length - i) % 3 == 0)
                    result.Append(' ');
                result.Append(digits[i]);
            }
            return sign + result;
        }
    }
}
