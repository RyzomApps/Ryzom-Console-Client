using API;
using API.Commands;
using System.Collections.Generic;

namespace Client.Commands
{
    /// <summary>
    /// Get the sheet-ID from a sheet-name
    /// </summary>
    public class SheetIdFromName : CommandBase
    {
        public override string CmdName => "GetSheetID";

        public override string CmdUsage => "<sheet file name>";

        public override string CmdDesc => "Get the sheet-ID from a sheet-name";

        public override bool Run(IClient handler, string command, out string responseMsg, Dictionary<string, object> localVars)
        {
            responseMsg = "";
            var args = GetArgs(command);

            if (args.Length != 1)
            {
                responseMsg = "Please specify a sheet-name.";
                return false;
            }

            var id = handler.GetApiSheetIdFactory().SheetId(args[0]);

            responseMsg = id.AsInt().ToString();
            return true;
        }
    }
}