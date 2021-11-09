using RCC.Chat;
using System.Collections.Generic;
using RCC.Commands.Internal;

namespace RCC.Commands
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

        public override string Run(RyzomClient handler, string command, Dictionary<string, object> localVars)
        {
            var args = GetArgs(command);

            handler.Channel = ChatGroupType.Universe;

            if (args.Length == 0)
                return "";

            var text = string.Join(" ", args);

            handler.SendText(text);

            return "";
        }
    }
}