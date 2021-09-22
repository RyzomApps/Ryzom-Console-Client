using System.Collections.Generic;
using RCC.Commands.Internal;
using RCC.Network;

namespace RCC.Commands
{
    public class Who : CommandBase
    {
        public override string CmdName => "who";
        public override string CmdUsage => "[<options (GM, channel name)>]";
        public override string CmdDesc => "Display all players currently in region";

        public override string Run(RyzomClient handler, string command, Dictionary<string, object> localVars)
        {
            // Check parameters.
            if (getArgs(command).Length > 1)
                return "";

            var out2 = new BitMemoryStream();

            if (!handler.GetNetworkManager().GetMessageHeaderManager().PushNameToStream("DEBUG:WHO", out2))
            {
                RyzomClient.Log?.Warn("Unknown message name DEBUG:WHO");
                return "";
            }

            var opt = "";

            if (getArgs(command).Length == 1)
            {
                opt = getArgs(command)[0];
            }

            out2.Serial(ref opt);
            handler.GetNetworkManager().Push(out2);
            return "";
        }

        public override IEnumerable<string> getCMDAliases()
        {
            return new[] {""};
        }
    }
}