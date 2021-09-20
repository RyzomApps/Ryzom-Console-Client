using RCC.Chat;
using System.Collections.Generic;

namespace RCC.Commands
{
    public class Guild : Command
    {
        public override string CmdName => "guild";
        public override string CmdUsage => "<text>";
        public override string CmdDesc => "With this command a message is sent to the guild channel, visible to all who are in the same guild as you.";

        public override IEnumerable<string> getCMDAliases()
        {
            return new[] { "g", "gu" };
        }

        public override string Run(RyzomClient handler, string command, Dictionary<string, object> localVars)
        {
            var args = getArgs(command);

            ((RyzomClient)RyzomClient.GetInstance()).Channel = ChatGroupType.Guild;

            if (args.Length == 0)
                return "";

            var text = string.Join(" ", args);

            ((RyzomClient)RyzomClient.GetInstance()).SendText(text);

            return "";
        }
    }
}