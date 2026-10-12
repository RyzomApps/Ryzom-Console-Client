///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

using API.Compass;
using System;
using System.Collections.Generic;

namespace Client.Compass
{
    /// <summary>
    /// Tracks the compass landmarks the server sends via the JOURNAL impulses
    /// (mission targets,place markers, tracked bots).
    /// </summary>
    public class CompassManager : ICompassManager
    {
        private readonly RyzomClient _client;
        private readonly List<CompassEntry> _entries = new List<CompassEntry>();

        /// <inheritdoc />
        public event Action OnCompassChanged;

        public CompassManager(RyzomClient client)
        {
            _client = client;
        }

        /// <summary>All current entries (read-only copy for callers).</summary>
        public IReadOnlyList<CompassEntry> Entries => _entries;

        /// <inheritdoc />
        public IReadOnlyList<CompassEntry> GetEntries() => _entries;

        /// <summary>
        /// Handle JOURNAL:ADD_COMPASS: x, y (sint32 world coords), text (uint string id).
        /// </summary>
        public void AddCompass(int x, int y, uint textId)
        {
            // The official client tolerates duplicates: replace an existing
            // entry with the same string id instead of adding a second one.
            for (var i = 0; i < _entries.Count; i++)
            {
                if (_entries[i].TextId == textId)
                {
                    _entries[i] = new CompassEntry { X = x, Y = y, TextId = textId, BotCompressedIndex = uint.MaxValue, IsBot = false };
                    OnCompassChanged?.Invoke();
                    return;
                }
            }

            _entries.Add(new CompassEntry { X = x, Y = y, TextId = textId, BotCompressedIndex = uint.MaxValue, IsBot = false });
            OnCompassChanged?.Invoke();
        }

        /// <summary>
        /// Handle JOURNAL:ADD_COMPASS_BOT: x, y, text and the compressed bot index.
        /// </summary>
        public void AddCompassBot(int x, int y, uint textId, uint botCompressedIndex)
        {
            for (var i = 0; i < _entries.Count; i++)
            {
                if (_entries[i].TextId == textId)
                {
                    _entries[i] = new CompassEntry { X = x, Y = y, TextId = textId, BotCompressedIndex = botCompressedIndex, IsBot = true };
                    OnCompassChanged?.Invoke();
                    return;
                }
            }

            _entries.Add(new CompassEntry { X = x, Y = y, TextId = textId, BotCompressedIndex = botCompressedIndex, IsBot = true });
            OnCompassChanged?.Invoke();
        }

        /// <summary>
        /// Handle JOURNAL:REMOVE_COMPASS: remove the entry whose string id
        /// matches the given one (removal is by string id, like the official
        /// client's CCompassDialogsManager::removeEntry).
        /// </summary>
        public void RemoveCompass(uint textId)
        {
            for (var i = 0; i < _entries.Count; i++)
            {
                if (_entries[i].TextId == textId)
                {
                    _entries.RemoveAt(i);
                    OnCompassChanged?.Invoke();
                    return;
                }
            }
        }

        /// <summary>Clear all entries (called on disconnect/reload of the string cache).</summary>
        public void Clear()
        {
            if (_entries.Count == 0)
                return;

            _entries.Clear();
            OnCompassChanged?.Invoke();
        }
    }
}
