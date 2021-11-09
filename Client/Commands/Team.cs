using System.Collections.Generic;
using Client.Chat;
using Client.Commands.Internal;

namespace Client.Commands
{
    public class Team : CommandBase
    {
        public override string CmdName => "team";
        public override string CmdUsage => "<text>";
        public override string CmdDesc => "With this command a message is sent to the Team channel and is visible to those currently in your party.";

        public override IEnumerable<string> GetCmdAliases()
        {
            return new[] { "p", "party", "te" };
        }

        public override string Run(RyzomClient handler, string command, Dictionary<string, object> localVars)
        {
            var args = GetArgs(command);

            handler.Channel = ChatGroupType.Team;

            if (args.Length == 0)
                return "";

            var text = string.Join(" ", args);

            handler.SendText(text);

            return "";
        }
    }
}