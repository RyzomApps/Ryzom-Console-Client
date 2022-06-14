///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

namespace Client.Phrase
{
    internal class PhraseClient
    {
        public int Version;
        public bool Lock;

        public PhraseClient()
        {
            Version = 0;
            Lock = false;
        }
    }
}