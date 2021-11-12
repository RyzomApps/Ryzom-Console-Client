using System;
using System.Collections.Generic;
using API;
using API.Commands;
using Client.Chat;

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

        public override string Run(IClient handler, string command, Dictionary<string, object> localVars)
        {
            if (!(handler is RyzomClient ryzomClient))
                throw new Exception("Command handler is not a Ryzom client.");

            var args = GetArgs(command);

            ryzomClient.Channel = ChatGroupType.Team;

            if (args.Length == 0)
                return "";

            var text = string.Join(" ", args);

            ryzomClient.SendText(text);

            return "";
        }
    }
}