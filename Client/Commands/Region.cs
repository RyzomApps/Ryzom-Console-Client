using API;
using API.Chat;
using API.Commands;
using System;
using System.Collections.Generic;

namespace Client.Commands
{
    public class Region : CommandBase
    {

        public override CommandCategory CmdCategory => CommandCategory.Info;
        public override string CmdName => "region";
        public override bool CmdAsync => true;

        public override string CmdUsage => "[text]";

        public override string CmdDesc =>
            "Select the region channel (no argument) or send a message in it; visible to all in the same region.";

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
