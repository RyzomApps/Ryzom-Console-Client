using API;
using API.Commands;
using System;
using System.Collections.Generic;

namespace Client.Commands
{
    public class SheetFind : CommandBase
    {
        public override string CmdName => "FindSheet";
        public override string CmdUsage => "<search pattern>";
        public override string CmdDesc => "Display all sheets matching the search pattern.";

        public override bool Run(IClient handler, string command, out string responseMsg, Dictionary<string, object> localVars)
        {
            responseMsg = "";
            if (handler is not RyzomClient ryzomClient)
                throw new Exception("Command handler is not a Ryzom client.");

            var args = GetArgs(command);

            // Check parameters
            if (args.Length == 0)
            {
                responseMsg = $"Usage: {CmdUsage}";
                return false;
            }

            var pattern = args[0];

            // Log entities
            responseMsg = ryzomClient.GetSheetIdFactory().FindSheet(pattern);
            return true;
        }

        public override IEnumerable<string> GetCmdAliases()
        {
            return ["SheetFind"];
        }
    }
}