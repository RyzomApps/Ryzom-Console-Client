using System;
using System.Collections.Generic;
using API;
using API.Commands;
using Client.Network;
using Client.Stream;

namespace Client.Commands
{
    public class Who : CommandBase
    {
        public override string CmdName => "who";
        public override string CmdUsage => "[GM|channelName]";
        public override string CmdDesc => "Display all players currently in region";

        public override string Run(IClient handler, string command, Dictionary<string, object> localVars)
        {
            if (!(handler is RyzomClient ryzomClient))
                throw new Exception("Command handler is not a Ryzom client.");

            // Check parameters.
            if (GetArgs(command).Length > 1)
                return "";

            const string msgName = "DEBUG:WHO";
            var out2 = new BitMemoryStream();

            if (!ryzomClient.GetNetworkManager().GetMessageHeaderManager().PushNameToStream(msgName, out2))
            {
                return $"Unknown message named '{msgName}'.";
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