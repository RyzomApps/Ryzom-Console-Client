using System.Collections.Generic;

namespace RCC.Client
{
    /// <summary>
    /// interface for action handlers
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