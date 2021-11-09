using System.Collections.Generic;
using Client.Commands.Internal;
using Client.Network;

namespace Client.Commands
{
    public class Who : CommandBase
    {
        public override string CmdName => "who";
        public override string CmdUsage => "[<options (GM, channel name)>]";
        public override string CmdDesc => "Display all players currently in region";

        public override string Run(RyzomClient handler, string command, Dictionary<string, object> localVars)
        {
            // Check parameters.
            if (GetArgs(command).Length > 1)
                return "";

            var out2 = new BitMemoryStream();

            if (!handler.GetNetworkManager().GetMessageHeaderManager().PushNameToStream("DEBUG:WHO", out2))
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
            handler.GetNetworkManager().Push(out2);
            return "";
        }

        public override IEnumerable<string> GetCmdAliases()
        {
            return new[] {""};
        }
    }
}