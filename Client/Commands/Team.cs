using System;
using System.Collections.Generic;
using API;
using API.Chat;
using API.Commands;

namespace Client.Commands
{
    public class Team : CommandBase
    {
        public override string CmdName => "team";

        public override string CmdUsage => "<text>";

        public override string CmdDesc =>
            "With this command a message is sent to the Team channel and is visible to those currently in your party.";

        public override bool Run(IClient handler, string command, out string responseMsg, Dictionary<string, object> localVars)
        {
            responseMsg = "";
            if (handler is not RyzomClient ryzomClient)
                throw new Exception("Command handler is not a Ryzom client.");

            var args = GetArgs(command);

            ryzomClient.Channel = ChatGroupType.Team;

            if (args.Length == 0)
                return true;

            var text = string.Join(" ", args);

            ryzomClient.SendText(text);

            return true;
        }

        public override IEnumerable<string> GetCmdAliases()
        {
            return ["p", "party", "te"];
        }
    }
}