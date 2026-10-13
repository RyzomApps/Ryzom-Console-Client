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
using System.Collections.Generic;
using System.Threading;

namespace Client.Commands
{
    /// <summary>
    /// Helper to synchronously resolve a server string: request it via
    /// STRING_RQ and pump the network until STRING_RESP arrives (the
    /// official client resolves dynamic ids the same way, just async).
    /// </summary>
    internal static class StringWait
    {
        private const int TimeoutMs = 5000;

        public static string WaitString(StringManager stringManager, uint stringId, NetworkManager networkManager)
        {
            return WaitAll(stringManager, new[] { stringId }, networkManager)[stringId];
        }

        /// <summary>
        /// Resolve several string ids in one go: registers the string manager's
        /// WaitString callbacks (upstream CStringManagerClient::waitString),
        /// then pumps the network until every callback fired or timeout.
        /// Requests already in the cache return instantly via the callback.
        /// </summary>
        public static Dictionary<uint, string> WaitAll(StringManager stringManager, IEnumerable<uint> stringIds, NetworkManager networkManager)
        {
            var pending = new HashSet<uint>();
            var results = new Dictionary<uint, string>();

            foreach (var id in stringIds)
            {
                if (id == 0)
                {
                    results[id] = string.Empty;
                    continue;
                }

                // GetString() without cache hit sends the STRING_RQ.
                if (stringManager.GetString(id, out var cached, networkManager))
                {
                    results[id] = cached;
                }
                else
                {
                    pending.Add(id);

                    // register the manager's async callback (fires from
                    // ReceiveString when STRING_RESP arrives)
                    stringManager.WaitString(id, (sid, value) =>
                    {
                        results[sid] = value;
                        pending.Remove(sid);
                    }, networkManager);
                }
            }

            if (pending.Count == 0)
                return results;

            var stopwatch = Stopwatch.StartNew();
            while (pending.Count > 0 && stopwatch.ElapsedMilliseconds < TimeoutMs)
            {
                networkManager.Update();

                // Update() receives and dispatches but does not flush the
                // outgoing queue — the main loop does that via Send(). Since
                // this helper blocks the main thread, flush here or the
                // STRING_RQ never reaches the frontend and the probe cycles.
                networkManager.Send(networkManager.GetNetworkConnection().GetCurrentServerTick());

                System.Threading.Thread.Sleep(10);
            }

            // anything unresolved falls back to the raw id
            foreach (var id in pending)
                results[id] = $"TEXT {id}";

            return results;
        }

        /// <summary>
        /// Non-blocking variant: requests every missing string and returns
        /// immediately. If all ids are already cached the results are returned
        /// synchronously (Completed = true). Otherwise the responses are
        /// collected via the string manager's WaitString callbacks (they fire
        /// from the main loop when STRING_RESP arrives) and Wait() can be used
        /// on a background task to block until all arrived or the timeout
        /// expires. Unresolved ids fall back to their raw "TEXT id" form.
        /// </summary>
        public static AsyncStringWait WaitAllAsync(StringManager stringManager, IEnumerable<uint> stringIds, NetworkManager networkManager)
        {
            var waiter = new AsyncStringWait();
            var pending = waiter.PendingIds;
            var results = waiter.Results;

            foreach (var id in stringIds)
            {
                if (id == 0)
                {
                    results[id] = string.Empty;
                    continue;
                }

                // GetString() without cache hit sends the STRING_RQ.
                if (stringManager.GetString(id, out var cached, networkManager))
                    results[id] = cached;
                else
                {
                    pending.Add(id);

                    // register the manager's async callback (fires from
                    // ReceiveString when STRING_RESP arrives)
                    stringManager.WaitString(id, (sid, value) =>
                    {
                        lock (waiter.Lock)
                        {
                            results[sid] = value;
                            pending.Remove(sid);

                            if (pending.Count == 0)
                                waiter.CompletedEvent.Set();
                        }
                    }, networkManager);
                }
            }

            waiter.Completed = pending.Count == 0;
            if (waiter.Completed)
                waiter.CompletedEvent.Set();

            return waiter;
        }
    }

    /// <summary>
    /// Handle for an asynchronous string resolution batch (see StringWait.WaitAllAsync).
    /// </summary>
    internal sealed class AsyncStringWait
    {
        public const int TimeoutMs = 5000;

        internal readonly object Lock = new();
        internal readonly ManualResetEventSlim CompletedEvent = new(false);
        internal readonly HashSet<uint> PendingIds = new();

        /// <summary>Results collected so far; filled synchronously for cached ids, then by the STRING_RESP callbacks.</summary>
        public readonly Dictionary<uint, string> Results = new();

        /// <summary>True if every id was already cached and Results is final (no Wait() needed).</summary>
        public bool Completed;

        /// <summary>
        /// Block until all strings arrived or the timeout expires. Call this
        /// from the command's background task, never from the main thread.
        /// Unresolved ids fall back to their raw "TEXT id" form.
        /// </summary>
        public Dictionary<uint, string> Wait(int timeoutMs = TimeoutMs)
        {
            if (!CompletedEvent.Wait(timeoutMs))
            {
                // timeout: whatever is still pending falls back to the raw id
                lock (Lock)
                {
                    foreach (var id in PendingIds)
                        Results[id] = $"TEXT {id}";
                    PendingIds.Clear();
                }
            }

            return Results;
        }
    }
}
