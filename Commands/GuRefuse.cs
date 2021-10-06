﻿using System.Collections.Generic;
using RCC.Commands.Internal;
using RCC.Network;

namespace RCC.Commands
{
    public class GuRefuse : CommandBase
    {
        public override string CmdName => "GURefuse";

        public override string CmdUsage => "";

        public override string CmdDesc => "refuse an invitation";

        public override string Run(RyzomClient handler, string command, Dictionary<string, object> localVars)
        {
            var args = GetArgs(command);

            if (args.Length != 0) return "";

            const string msgName = "GUILD:REFUSE_INVITATION";
            var out2 = new BitMemoryStream();

            if (handler.GetNetworkManager().GetMessageHeaderManager().PushNameToStream(msgName, out2))
                handler.GetNetworkManager().Push(out2);
            return "";
        }

        public override IEnumerable<string> GetCmdAliases()
        {
            return new string[] { };
        }
    }
}