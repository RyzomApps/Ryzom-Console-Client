using RCC.Chat;
using System.Collections.Generic;
using RCC.Commands.Internal;

namespace RCC.Commands
{
    public class Region : CommandBase
    {
        public override string CmdName => "region";
        public override string CmdUsage => "<text>";
        public override string CmdDesc => "This command sends a message visible to all who are in the same region as you at the time.";

        public override IEnumerable<string> getCMDAliases()
        {
            return new[] { "r", "re" };
        }

        public override string Run(RyzomClient handler, string command, Dictionary<string, object> localVars)
        {
            var args = getArgs(command);

            ((RyzomClient)RyzomClient.GetInstance()).Channel = ChatGroupType.Region;

            if (args.Length == 0)
                return "";

            var text = string.Join(" ", args);

            ((RyzomClient)RyzomClient.GetInstance()).SendText(text);

            return "";
        }
    }
}