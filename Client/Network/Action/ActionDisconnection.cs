///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

namespace Client.Network.Action
{
    /// <summary>
    /// This action means the entity Id has left the game.
    /// </summary>
    /// <remarks>Note: No more data or processing than in CAction.</remarks>
    /// <author>Olivier Cado</author>
    /// <author>Nevrax France</author> 
    /// <date>2001</date> 
    public class ActionDisconnection : ActionImpulsion
    {
        public static ActionBase Create()
        {
            return new ActionDisconnection();
        }

        public override void Unpack(BitMemoryStream message)
        {

        }

        public override int Size()
        {
            return 0;
        }

        /// <summary>
        /// This method intialises the action with a default state
        /// </summary>
        public override void Reset()
        {
            AllowExceedingMaxSize = false;
        }

        public override void Pack(BitMemoryStream message)
        {

        }
    }
}

