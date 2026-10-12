///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////
using Client.Network;
using Client.Strings;
using System;
using System.Diagnostics;

namespace Client.Commands
{
    /// <summary>
    /// Helper to synchronously resolve a server string: request it via
    /// STRING_RQ and pump the network until STRING_RESP arrives (the
    /// official client resolves dynamic ids the same way, just async).
    /// </summary>
    internal static class StringWait
    {
        private const int TimeoutMs = 2000;

        public static string WaitString(StringManager stringManager, uint stringId, NetworkManager networkManager)
        {
            if (stringManager.GetString(stringId, out var result, networkManager))
                return result;

            var stopwatch = Stopwatch.StartNew();
            while (stopwatch.ElapsedMilliseconds < TimeoutMs)
            {
                if (stringManager.GetString(stringId, out result, networkManager))
                    return result;

                networkManager.Update();
                System.Threading.Thread.Sleep(10);
            }

            return $"TEXT {stringId}";
        }
    }
}
