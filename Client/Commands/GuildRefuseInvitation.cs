﻿using System;
using System.Collections.Generic;
using API;
using API.Commands;
using Client.Network;

namespace Client.Commands
{
    public class GuildRefuseInvitation : CommandBase
    {
        public override string CmdName => "GuildRefuseInvitation";

        public override string CmdUsage => "";

        public override string CmdDesc => "refuse an invitation";

        public override string Run(IClient handler, string command, Dictionary<string, object> localVars)
        {
            if (!(handler is RyzomClient ryzomClient))
                throw new Exception("Command handler is not a Ryzom client.");

            var args = GetArgs(command);

            if (args.Length != 0) return "";

            const string msgName = "GUILD:REFUSE_INVITATION";
            var out2 = new BitMemoryStream();

            if (ryzomClient.GetNetworkManager().GetMessageHeaderManager().PushNameToStream(msgName, out2))
                ryzomClient.GetNetworkManager().Push(out2);

            return "";
        }

        public override IEnumerable<string> GetCmdAliases()
        {
            return new string[] { };
        }
    }
}