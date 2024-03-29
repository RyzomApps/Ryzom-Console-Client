﻿using System;
using System.Collections.Generic;
using API;
using API.Commands;

namespace Client.Commands
{
    public class Kill : CommandBase
    {
        public override string CmdName => "selfkill";
        public override string CmdUsage => "";
        public override string CmdDesc => "Kill the player";

        public override string Run(IClient handler, string command, Dictionary<string, object> localVars)
        {
            if (!(handler is RyzomClient ryzomClient))
                throw new Exception("Command handler is not a Ryzom client.");

            ryzomClient.GetNetworkManager().SendMsgToServer("COMMAND:SELFKILL");

            return "";
        }

        public override IEnumerable<string> GetCmdAliases()
        {
            return new[] {"kill"};
        }
    }
}