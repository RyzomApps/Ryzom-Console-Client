﻿using RCC.Chat;
using System.Collections.Generic;
using RCC.Commands.Internal;

namespace RCC.Commands
{
    public class Shout : CommandBase
    {
        public override string CmdName => "shout";
        public override string CmdUsage => "<text>";
        public override string CmdDesc => "This command will make your messages have a 50m range and will appear red (by default) for you.";

        public override IEnumerable<string> GetCmdAliases()
        {
            return new[] { "sh", "y", "yell" };
        }

        public override string Run(RyzomClient handler, string command, Dictionary<string, object> localVars)
        {
            var args = GetArgs(command);

            handler.Channel = ChatGroupType.Shout;

            if (args.Length == 0)
                return "";

            var text = string.Join(" ", args);

            handler.SendText(text);

            return "";
        }
    }
}