///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Diagnostics;

namespace Client.Database
{
    /// <summary>
    /// Text id
    /// </summary>
    /// <author>Stephane Coutelas</author>
    /// <author>Nevrax France</author>
    /// <date>2002</date>
    public class TextId
    {
        private readonly List<string> _ids = new List<string>();
        private int _idx;

        /// <summary>
        /// Default constructor
        /// </summary>
        public TextId()
        {
            _idx = 0;
        }

        /// <summary>
        /// Init this text id from a string
        /// </summary>
        public TextId(string str)
        {
            _idx = 0;
            var s = str;
            int i;
            int j;
            for (i = 0, j = 0; i + j < s.Length; j++)
                if (s[i + j] == ':')
                {
                    _ids.Add(s.Substring(i, j));
                    i += j + 1; // +1 to skip the ':'
                    j = 0;
                }

            // deal with the last id in the string (terminated by a '\x0' and not a ':')
            _ids.Add(s.Substring(i, j));
        }

        /// <summary>
        /// Build a string from this text id
        /// </summary>
        public override string ToString()
        {
            if (_ids.Count == 0) return "";
            var str = _ids[0];
            for (var i = 1; i < _ids.Count; i++) str += ":" + _ids[i];
            return str;
        }

        /// <summary>
        /// Push back a sub name id to this id
        /// </summary>
        public void Push(string str)
        {
            _ids.Add(str);
        }

        /// <summary>
        /// Remove the last sub name id to this id
        /// </summary>
        public void Pop()
        {
            _ids.RemoveAt(_ids.Count - 1);
        }

        /// <summary>
        /// Return the next sub id
        /// </summary>
        public string ReadNext()
        {
            Debug.Assert(_idx < _ids.Count);
            return _ids[_idx++];
        }

        /// <summary>
        /// return true if a call to readNext can be performed
        /// </summary>
        public bool HasElements()
        {
            return _idx < _ids.Count;
        }

        /// <summary>
        /// Get the current index in Id
        /// </summary>
        public int GetCurrentIndex()
        {
            return _idx;
        }

        /// <summary>
        /// Return the count of strings composing this id
        /// </summary>
        public uint Size()
        {
            return (uint)_ids.Count;
        }

        /// <summary>
        /// Return an element. empty if bad index
        /// </summary>
        public string GetElement(int idx)
        {
            return idx >= Size() ? string.Empty : _ids[idx];
        }
    }
}