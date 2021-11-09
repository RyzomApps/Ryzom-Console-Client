using RCC.Chat;
using System.Collections.Generic;
using RCC.Commands.Internal;

namespace RCC.Commands
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

        public override string Run(RyzomClient handler, string command, Dictionary<string, object> localVars)
        {
            var args = GetArgs(command);

            handler.Channel = ChatGroupType.Guild;

            if (args.Length == 0)
                return "";

            var text = string.Join(" ", args);

            handler.SendText(text);

            return "";
        }
    }
}