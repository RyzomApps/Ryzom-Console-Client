using API;
using API.Chat;
using API.Commands;
using System;
using System.Collections.Generic;

namespace Client.Commands
{
    public class Universe : CommandBase
    {
        public override string CmdName => "universe";

        public override string CmdUsage => "[text]";

        public override string CmdDesc =>
            "This command sends a message to the universe channel, which is visible to everyone online at that moment.";

        public override bool Run(IClient handler, string command, out string responseMsg, Dictionary<string, object> localVars)
        {
            responseMsg = "";
            if (handler is not RyzomClient ryzomClient)
                throw new Exception("Command handler is not a Ryzom client.");

            var args = GetArgs(command);

            ryzomClient.Channel = ChatGroupType.Universe;

            if (args.Length == 0)
                return true;

            var text = string.Join(" ", args);

            ryzomClient.SendText(text);

            return true;
        }

        public override IEnumerable<string> GetCmdAliases()
        {
            return ["u"];
        }
    }
}