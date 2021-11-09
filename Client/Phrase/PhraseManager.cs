///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

namespace Client.Phrase
{
    /// <summary>
    /// Singleton to Get/Set Sabrina Phrase in SpellBook / Memory.
    /// </summary>
    /// <remarks>NB: you MUST create it (getInstance()), before loading of ingame.xmls.</remarks>
    /// <author>Lionel Berenguier</author>
    /// <author>Nevrax France</author>
    /// <date>2003</date>
    public class PhraseManager
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public PhraseManager()
        {
            //reset();
            //for(uint i=0;i<NumSuccessTable;i++)
            //    _SuccessTableSheet[i]= NULL;
            //_RegenTickRangeTouched = true;
        }

        /// <summary>
        /// To call when The DB inGame is setup. Else, no write is made to it before. (NB: DB is updated here)
        /// </summary>
        public void InitInGame()
        {
            //throw new System.NotImplementedException();
        }
    }
}