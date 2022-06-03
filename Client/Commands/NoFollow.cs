﻿using System;
using System.Collections.Generic;
using API;
using API.Commands;
using Client.Network;

namespace Client.Commands
{
    public class NoFollow : CommandBase
    {
        public override string CmdName => "noFollow";
        public override string CmdUsage => "";
        public override string CmdDesc => "Stop following the target";

        public override string Run(IClient handler, string command, Dictionary<string, object> localVars)
        {
            if (!(handler is RyzomClient ryzomClient))
                throw new Exception("Command handler is not a Ryzom client.");

            // Check parameters.
            if (HasArg(command)) return "";

            // Create the message and send.
            const string msgName = "TARGET:NO_FOLLOW";
            var out2 = new BitMemoryStream();
                
            if (ryzomClient.GetNetworkManager().GetMessageHeaderManager().PushNameToStream(msgName, out2))
            {
                ryzomClient.GetNetworkManager().Push(out2);
            }
            else
                return $"Unknown message named '{msgName}'.";

            return "";
        }

        public override IEnumerable<string> GetCmdAliases()
        {
            return new string[] { };
        }
    }
}