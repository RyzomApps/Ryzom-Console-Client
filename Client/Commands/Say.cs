using System;
using System.Collections.Generic;
using API;
using API.Chat;
using API.Commands;

namespace Client.Commands
{
    public class Say : CommandBase
    {
        public override string CmdName => "say";

        public override string CmdUsage => "<text>";

        public override string CmdDesc => "Use the around channel for messages. Messages sent normally in the around channel have a 25m range.";

        public override string Run(IClient handler, string command, Dictionary<string, object> localVars)
        {
            if (!(handler is RyzomClient ryzomClient))
                throw new Exception("Command handler is not a Ryzom client.");

            var args = GetArgs(command);

            ryzomClient.Channel = ChatGroupType.Around;

            if (args.Length == 0)
                return "";

            var text = string.Join(" ", args);

            ryzomClient.SendText(text);

            return "";
        }

        public override IEnumerable<string> GetCmdAliases()
        {
            return new[] { "s" };
        }
    }
}