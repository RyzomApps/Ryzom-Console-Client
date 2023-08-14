using System;
using System.Collections.Generic;
using API;
using API.Commands;
using Client.Stream;

namespace Client.Commands
{
    public class SheetFind : CommandBase
    {
        public override string CmdName => "FindSheet";
        public override string CmdUsage => "<search pattern>";
        public override string CmdDesc => "Display all sheets matching the search pattern.";

        public override string Run(IClient handler, string command, Dictionary<string, object> localVars)
        {
            if (!(handler is RyzomClient ryzomClient))
                throw new Exception("Command handler is not a Ryzom client.");

            var args = GetArgs(command);

            // Check parameters
            if (args.Length == 0)
                return "Usage: " + CmdUsage;

            var pattern = args[0];

            // Log entities
            return ryzomClient.GetSheetIdFactory().FindSheet(pattern);
        }

        public override IEnumerable<string> GetCmdAliases()
        {
            return new[] {"SheetFind"};
        }
    }
}