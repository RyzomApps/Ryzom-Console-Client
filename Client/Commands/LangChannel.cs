using System;
using System.Collections.Generic;
using API;
using API.Commands;

namespace Client.Commands
{
    public class LangChannel : CommandBase
    {
        public override string CmdName => "LangChannel";

        public override string CmdUsage => "<fr|en|de|es|ru>";

        public override string CmdDesc => "Join and leave \"per language\" universe channels.";

        public override string Run(IClient handler, string command, Dictionary<string, object> localVars)
        {
            if (!(handler is RyzomClient ryzomClient))
                throw new Exception("Command handler is not a Ryzom client.");

            var args = GetArgs(command);

            // Check parameters
            if (args.Length < 1)
                return "Wrong argument count in the command.";

            // Perform admin command
            var response = "";
            ryzomClient.PerformInternalCommand("a connectLangChannel " + string.Join(' ', args), ref response);
            return response;
        }

        public override IEnumerable<string> GetCmdAliases()
        {
            return new[] { "" };
        }
    }
}