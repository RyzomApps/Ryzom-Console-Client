using System;
using System.Collections.Generic;
using API;
using API.Commands;
using Client.Chat;

namespace Client.Commands
{
    public class Universe : CommandBase
    {
        public override string CmdName => "universe";
        public override string CmdUsage => "<text>";
        public override string CmdDesc => "This command sends a message to the universe channel, which is visible to everyone online at that moment.";

        public override IEnumerable<string> GetCmdAliases()
        {
            return new[] { "u" };
        }

        public override string Run(IClient handler, string command, Dictionary<string, object> localVars)
        {
            if (!(handler is RyzomClient ryzomClient))
                throw new Exception("Command handler is not a Ryzom client.");

            var args = GetArgs(command);

            ryzomClient.Channel = ChatGroupType.Universe;

            if (args.Length == 0)
                return "";

            var text = string.Join(" ", args);

            ryzomClient.SendText(text);

            return "";
        }
    }
}