using System;
using System.Collections.Generic;
using API;
using API.Chat;
using API.Commands;

namespace Client.Commands
{
    public class Shout : CommandBase
    {
        public override string CmdName => "shout";

        public override string CmdUsage => "[text]";

        public override string CmdDesc =>
            "This command will make your messages have a 50m range and will appear red (by default) for you.";

        public override string Run(IClient handler, string command, Dictionary<string, object> localVars)
        {
            if (!(handler is RyzomClient ryzomClient))
                throw new Exception("Command handler is not a Ryzom client.");

            var args = GetArgs(command);

            ryzomClient.Channel = ChatGroupType.Shout;

            if (args.Length == 0)
                return "";

            var text = string.Join(" ", args);

            ryzomClient.SendText(text);

            return "";
        }

        public override IEnumerable<string> GetCmdAliases()
        {
            return new[] {"sh", "y", "yell"};
        }
    }
}