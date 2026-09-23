using API;
using API.Chat;
using API.Commands;
using System;
using System.Collections.Generic;

namespace Client.Commands
{
    public class Region : CommandBase
    {
        public override string CmdName => "region";

        public override string CmdUsage => "[text]";

        public override string CmdDesc =>
            "This command sends a message visible to all who are in the same region as you at the time.";

        public override bool Run(IClient handler, string command, out string responseMsg, Dictionary<string, object> localVars)
        {
            responseMsg = "";
            if (handler is not RyzomClient ryzomClient)
                throw new Exception("Command handler is not a Ryzom client.");

            var args = GetArgs(command);

            ryzomClient.Channel = ChatGroupType.Region;

            if (args.Length == 0)
                return true;

            var text = string.Join(" ", args);

            ryzomClient.SendText(text);

            return true;
        }

        public override IEnumerable<string> GetCmdAliases()
        {
            return ["r", "re"];
        }
    }
}