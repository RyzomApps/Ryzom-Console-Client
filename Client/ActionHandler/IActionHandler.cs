///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

using System.Collections.Generic;

namespace Client.ActionHandler
{
    /// <summary>
    /// interface for action handlers
    /// </summary>
    /// <author>Nicolas Brigand</author>
    /// <author>Nevrax France</author>
    /// <date>2002</date>
    public abstract class IActionHandler : System.IDisposable
    {
        // Execute the answer to the action
        // Params has the following form : paramName=theParam|paramName2=theParam2|...
        public abstract void Execute(object pCaller, in string sParams);

        public abstract void Dispose();

        public virtual string GetParam(in string @params, in string paramName)
        {
            return "";
        }

        public virtual void GetAllParams(in string @params, List<System.Tuple<string, string>> allParams) { }
    }
}