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

        public override string Run(IClient ryzomClient, string command, Dictionary<string, object> localVars)
        {
            var args = GetArgs(command);

            if (args.Length != 1)
            {
                ryzomClient.GetLogger().Warn("Please specify a sheet-ID.");
                return "";
            }

            if (!uint.TryParse(args[0], out var nId))
            {
                ryzomClient.GetLogger().Warn("Could not parse argument.");
                return "";
            }

            var id = ryzomClient.GetApiSheetIdFactory().SheetId(nId);

            return id.Name;
        }

        public override IEnumerable<string> GetCmdAliases()
        {
            return new[] { "" };
        }
    }
}