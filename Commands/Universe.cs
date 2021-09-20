using RCC.Chat;
using System.Collections.Generic;

namespace RCC.Commands
{
    public class Universe : Command
    {
        public override string CmdName => "universe";
        public override string CmdUsage => "<text>";
        public override string CmdDesc => "This command sends a message to the universe channel, which is visible to everyone online at that moment.";

        public override IEnumerable<string> getCMDAliases()
        {
            return new[] { "u" };
        }

        public override string Run(RyzomClient handler, string command, Dictionary<string, object> localVars)
        {
            var args = getArgs(command);

            ((RyzomClient)RyzomClient.GetInstance()).Channel = ChatGroupType.Universe;

            if (args.Length == 0)
                return "";

            var text = string.Join(" ", args);

            ((RyzomClient)RyzomClient.GetInstance()).SendText(text);

            return "";
        }
    }
}