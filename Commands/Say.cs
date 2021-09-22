using RCC.Chat;
using System.Collections.Generic;
using RCC.Commands.Internal;

namespace RCC.Commands
{
    public class Say : CommandBase
    {
        public override string CmdName => "say";
        public override string CmdUsage => "<text>";
        public override string CmdDesc => "Messages sent normally in the around channel have a 25m range.";

        public override IEnumerable<string> GetCmdAliases()
        {
            return new[] { "s" };
        }

        public override string Run(RyzomClient handler, string command, Dictionary<string, object> localVars)
        {
            var args = GetArgs(command);

            ((RyzomClient)RyzomClient.GetInstance()).Channel = ChatGroupType.Around;

            if (args.Length == 0)
                return "";

            var text = string.Join(" ", args);

            ((RyzomClient)RyzomClient.GetInstance()).SendText(text);

            return "";
        }
    }
}