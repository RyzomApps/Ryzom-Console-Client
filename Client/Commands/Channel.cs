using System;
using System.Collections.Generic;
using API;
using API.Chat;
using API.Commands;

namespace Client.Commands
{
    public class Channel : CommandBase
    {
        public override string CmdName => "channel";

        public override string CmdUsage => "<name> [text]";

        public override string CmdDesc => "Will attempt to change the channel to the specified one.";

        public override bool Run(IClient handler, string command, out string responseMsg, Dictionary<string, object> localVars)
        {
            responseMsg = "";
            if (handler is not RyzomClient ryzomClient)
                throw new Exception("Command handler is not a Ryzom client.");

            var args = GetArgs(command);

            if (args.Length == 0)
            {
                responseMsg = $"Please specify a channel name: {string.Join(", ", Enum.GetNames(typeof(ChatGroupType)))}.";
                return false;
            }

            if (!Enum.TryParse<ChatGroupType>(args[0], true, out var type))
            {
                responseMsg = $"Cannot find channel. Valid channels are: {string.Join(", ", Enum.GetNames(typeof(ChatGroupType)))}.";
                return false;
            }

            ryzomClient.Channel = type;

            if (args.Length <= 1)
                return true;

            // got some text to send
            var text = string.Join(" ", args[1..]);

            ryzomClient.SendText(text);

            return true;
        }

        public override IEnumerable<string> GetCmdAliases()
        {
            return ["sh", "y", "yell"];
        }
    }
}