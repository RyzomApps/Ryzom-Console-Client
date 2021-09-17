using System.Collections.Generic;

namespace RCC.Helper
{
    /// <summary>
    ///     Interface for TAB autocompletion
    ///     Allows to use any object which has an AutoComplete() method using the IAutocomplete interface
    /// </summary>
    public interface IAutoComplete
    {
        /// <summary>
        ///     Provide a list of auto-complete strings based on the provided input behing the cursor
        /// </summary>
        /// <param name="behindCursor">Text behind the cursor, e.g. "my input comm"</param>
        /// <returns>List of auto-complete words, e.g. ["command", "comment"]</returns>
        IEnumerable<string> AutoComplete(string behindCursor);
    }
}