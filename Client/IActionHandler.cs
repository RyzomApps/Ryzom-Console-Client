// This code is a modified version of a file from the 'Ryzom - MMORPG Framework'
// <http://dev.ryzom.com/projects/ryzom/>,
// which is released under GNU Affero General Public License.
// <http://www.gnu.org/licenses/>
// Original Copyright 2010 by Winch Gate Property Limited

namespace RCC.Client
{
    /// <summary>
    ///     interface for action handlers
    /// </summary>
    public interface IActionHandler
    {
        // Execute the answer to the action
        // Params has the following form : paramName=theParam|paramName2=theParam2|...
        //void execute(object pCaller, string sParams);

        //string getParam(string Params, string ParamName);

        //void getAllParams(string Params, List<KeyValuePair<string, string>> AllParams);
    }
}