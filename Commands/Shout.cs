using RCC.Chat;
using System.Collections.Generic;

namespace RCC.Commands
{
    public class Shout : Command
    {
        public override string CmdName => "shout";
        public override string CmdUsage => "<text>";
        public override string CmdDesc => "This command will make your messages have a 50m range and will appear red (by default) for you.";

        public override IEnumerable<string> getCMDAliases()
        {
            return new[] { "sh", "y", "yell" };
        }

        public override string Run(RyzomClient handler, string command, Dictionary<string, object> localVars)
        {
            var args = getArgs(command);

            ((RyzomClient)RyzomClient.GetInstance()).Channel = ChatGroupType.Shout;

            if (args.Length == 0)
                return "";

            var text = string.Join(" ", args);

            ((RyzomClient)RyzomClient.GetInstance()).SendText(text);

            return "";
        }
    }
}