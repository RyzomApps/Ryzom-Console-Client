///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////
using API;
using API.Commands;
using Client.Stream;
using System;
using System.Collections.Generic;
using System.Text;

namespace Client.Commands
{
    /// <summary>
    /// Safe accessors for the exchange database nodes (works without pre-existing branches).
    /// </summary>
    internal static class ExchangeDb
    {
        /// <summary>
        /// Null-safe property read. Returns fallback when the node does not exist (minimal config).
        /// </summary>
        public static long SafeProp(RyzomClient ryzomClient, string name, long fallback)
        {
            var node = ryzomClient.GetDatabaseManager().GetNode(name, false);
            return node?.GetValue64() ?? fallback;
        }
    }
}
