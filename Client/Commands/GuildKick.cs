﻿using System;
using System.Collections.Generic;
using API;
using API.Commands;
using Client.Network;

namespace Client.Commands
{
    public class GuildKick : CommandBase
    {
        public override string CmdName => "GuildKick";

        public override string CmdUsage => "<player name> <counter>";

        public override string CmdDesc => "client (lead,ho,of) wants to kick member specifying its index. Last param is the counter";

        public override string Run(IClient handler, string command, Dictionary<string, object> localVars)
        {
            if (!(handler is RyzomClient ryzomClient))
                throw new Exception("Command handler is not a Ryzom client.");

            var args = GetArgs(command);

            if (args.Length != 1) return "";

            const string msgName = "GUILD:KICK_MEMBER"; // TODO GUKick right arguments
            var out2 = new BitMemoryStream();

            if (!ryzomClient.GetNetworkManager().GetMessageHeaderManager().PushNameToStream(msgName, out2))
                return "";

            var buf = args[0];
            out2.Serial(ref buf);
            ryzomClient.GetNetworkManager().Push(out2);

            return "";
        }

        public override IEnumerable<string> GetCmdAliases()
        {
            return new string[] { };
        }
    }
}