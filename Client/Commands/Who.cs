using System;
using System.Collections.Generic;
using API;
using API.Commands;
using Client.Network;

namespace Client.Commands
{
    public class Who : CommandBase
    {
        public override string CmdName => "who";
        public override string CmdUsage => "[<options (GM, channel name)>]";
        public override string CmdDesc => "Display all players currently in region";

        public override string Run(IClient handler, string command, Dictionary<string, object> localVars)
        {
            if (!(handler is RyzomClient ryzomClient))
                throw new Exception("Command handler is not a Ryzom client.");

            // Check parameters.
            if (GetArgs(command).Length > 1)
                return "";

            var out2 = new BitMemoryStream();

            if (!ryzomClient.GetNetworkManager().GetMessageHeaderManager().PushNameToStream("DEBUG:WHO", out2))
            {
                handler.GetLogger().Warn("Unknown message name DEBUG:WHO");
                return "";
            }

            var opt = "";

            if (GetArgs(command).Length == 1)
            {
                opt = GetArgs(command)[0];
            }

            out2.Serial(ref opt);
            ryzomClient.GetNetworkManager().Push(out2);
            return "";
        }

        public override IEnumerable<string> GetCmdAliases()
        {
            return new[] {""};
        }
    }
}