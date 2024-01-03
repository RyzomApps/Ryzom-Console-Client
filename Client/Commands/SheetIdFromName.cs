using System.Collections.Generic;
using API;
using API.Commands;
using API.Sheet;

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

        public override string Run(IClient ryzomClient, string command, Dictionary<string, object> localVars)
        {
            var args = GetArgs(command);

            if (args.Length != 1)
            {
                ryzomClient.GetLogger().Warn("Please specify a sheet-name.");
                return "";
            }

            var id = ryzomClient.GetApiSheetIdFactory().SheetId(args[0]);

            return id.AsInt().ToString();
        }

        public override IEnumerable<string> GetCmdAliases()
        {
            return new[] { "" };
        }
    }
}