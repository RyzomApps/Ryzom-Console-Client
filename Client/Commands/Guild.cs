using System;
using System.Collections.Generic;
using API;
using API.Commands;
using Client.Chat;

namespace Client.Commands
{
    public class Guild : CommandBase
    {
        public override string CmdName => "guild";
        public override string CmdUsage => "<text>";
        public override string CmdDesc => "With this command a message is sent to the guild channel, visible to all who are in the same guild as you.";

        public override IEnumerable<string> GetCmdAliases()
        {
            return new[] { "g", "gu" };
        }

        public override string Run(IClient handler, string command, Dictionary<string, object> localVars)
        {
            if (!(handler is RyzomClient ryzomClient))
                throw new Exception("Command handler is not a Ryzom client.");

            var args = GetArgs(command);

            ryzomClient.Channel = ChatGroupType.Guild;

            if (args.Length == 0)
                return "";

            var text = string.Join(" ", args);

            ryzomClient.SendText(text);

            return "";
        }
    }
}