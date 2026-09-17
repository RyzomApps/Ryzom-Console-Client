using System.Collections.Generic;
using API;
using API.Commands;

namespace Client.Commands
{
    /// <summary>
    /// Get the sheet-name from a sheet-ID
    /// </summary>
    public class SheetNameFromId : CommandBase
    {
        public override string CmdName => "GetSheetName";

        public override string CmdUsage => "<Sheet Id>";

        public override string CmdDesc => "Get the sheet-name from a sheet-ID";

        public override bool Run(IClient handler, string command, out string responseMsg, Dictionary<string, object> localVars)
        {
            responseMsg = "";
            var args = GetArgs(command);

            if (args.Length != 1)
            {
                responseMsg = "Please specify a sheet-ID.";
                return false;
            }

            if (!uint.TryParse(args[0], out var nId))
            {
                responseMsg = "Could not parse argument.";
                return false;
            }

            var id = handler.GetApiSheetIdFactory().SheetId(nId);

            responseMsg = id.Name;
            return true;
        }
    }
}