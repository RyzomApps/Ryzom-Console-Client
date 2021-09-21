using System.Collections.Generic;
using RCC.Messages;
using RCC.Network;

namespace RCC.Commands
{
    public class Who : Command
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

            if (!GenericMessageHeaderManager.PushNameToStream("DEBUG:WHO", out2))
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
            NetworkManager.Push(out2);
            return "";
        }

        public override IEnumerable<string> getCMDAliases()
        {
            return new[] {""};
        }
    }
}