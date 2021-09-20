using RCC.Chat;
using System.Collections.Generic;

namespace RCC.Commands
{
    public class Team : Command
    {
        public override string CmdName => "team";
        public override string CmdUsage => "<text>";
        public override string CmdDesc => "With this command a message is sent to the Team channel and is visible to those currently in your party.";

        public override IEnumerable<string> getCMDAliases()
        {
            return new[] { "p", "party", "te" };
        }

        public override string Run(RyzomClient handler, string command, Dictionary<string, object> localVars)
        {
            var args = getArgs(command);

            ((RyzomClient)RyzomClient.GetInstance()).Channel = ChatGroupType.Team;

            if (args.Length == 0)
                return "";

            var text = string.Join(" ", args);

            ((RyzomClient)RyzomClient.GetInstance()).SendText(text);

            return "";
        }
    }
}