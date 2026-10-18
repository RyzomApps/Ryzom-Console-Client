using API;
using API.Chat;
using API.Commands;
using System;
using System.Collections.Generic;

namespace Client.Commands
{
    public class Universe : CommandBase
    {

        public override CommandCategory CmdCategory => CommandCategory.Chat;
        public override string CmdName => "universe";

        public override string CmdUsage => "[text]";

        public override string CmdDesc =>
            "Select the universe channel (no argument) or send a message in it; visible to everyone online.";

        public override bool CmdAsync => true;
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
