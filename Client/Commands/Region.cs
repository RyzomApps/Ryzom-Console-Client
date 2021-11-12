using System;
using System.Collections.Generic;
using API;
using API.Commands;
using Client.Chat;

namespace Client.Commands
{
    public class Region : CommandBase
    {
        public override string CmdName => "region";
        public override string CmdUsage => "<text>";
        public override string CmdDesc => "This command sends a message visible to all who are in the same region as you at the time.";

        public override IEnumerable<string> GetCmdAliases()
        {
            return new[] { "r", "re" };
        }

        public override string Run(IClient handler, string command, Dictionary<string, object> localVars)
        {
            if (!(handler is RyzomClient ryzomClient))
                throw new Exception("Command handler is not a Ryzom client.");

            var args = GetArgs(command);

            ryzomClient.Channel = ChatGroupType.Region;

            if (args.Length == 0)
                return "";

            var text = string.Join(" ", args);

            ryzomClient.SendText(text);

            return "";
        }
    }
}