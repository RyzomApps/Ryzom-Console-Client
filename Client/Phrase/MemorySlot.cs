///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

namespace Client.Phrase
{
    public class MemorySlot
    {
        /// <summary>
        /// Is this a macro
        /// </summary>
        public bool IsMacro;

        /// <summary>
        /// Is his visual dirty?
        /// </summary>
        public bool IsMacroVisualDirty;

        /// <summary>
        /// Macro or PhraseId
        /// </summary>
        public uint Id;

        public MemorySlot()
        {
            IsMacro = false;
            IsMacroVisualDirty = false;
            Id = 0;
        }

        public bool IsEmpty()
        {
            return Id == 0 && IsMacro == false;
        }

        public bool IsPhrase()
        {
            return !IsMacro && Id != 0;
        }
    }
}