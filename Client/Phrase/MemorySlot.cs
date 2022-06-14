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
        // Is this a macro
        public bool IsMacro;
        // Is his visual dirty?
        public bool IsMacroVisualDirty;
        // Macro or PhraseId
        public uint Id;
        public MemorySlot()
        {
            IsMacro = false;
            IsMacroVisualDirty = false;
            Id = 0;
        }

        public bool isEmpty()
        {
            return Id == 0 && IsMacro == false;
        }

        public bool isMacro()
        {
            return IsMacro;
        }

        public bool isPhrase()
        {
            return !IsMacro && Id != 0;
        }
    }
}