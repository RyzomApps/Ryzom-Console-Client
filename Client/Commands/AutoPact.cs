﻿using System;
using System.Collections.Generic;
using API;
using API.Commands;
using Client.Network;

namespace Client.Commands
{
    public class AutoPact : CommandBase
    {
        public override string CmdName => "AutoPact";
        public override string CmdUsage => "<u8>";
        public override string CmdDesc => "client want set AUTOPACT to TRUE or FALSE";
        public override IEnumerable<string> GetCmdAliases() { return new string[] { }; }

        public override string Run(IClient handler, string command, Dictionary<string, object> localVars)
        {
            if (!(handler is RyzomClient ryzomClient))
                throw new Exception("Command handler is not a Ryzom client.");

            var args = GetArgs(command);

            if (args.Length != 1)
                return "";

            // send command
            const string msgName = "COMMAND:AUTOPACT";

            var index = byte.Parse(args[0]);
            var out2 = new BitMemoryStream();

            if (ryzomClient.GetNetworkManager().GetMessageHeaderManager().PushNameToStream(msgName, out2))
            {
                out2.Serial(ref index);
                ryzomClient.GetNetworkManager().Push(out2);
            }
            else
                handler.GetLogger().Warn($"Unknown message named '{msgName}'.");

            return "";
        }
    }
}