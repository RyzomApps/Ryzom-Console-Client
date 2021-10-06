using System.Collections.Generic;
using RCC.Commands.Internal;
using RCC.Network;

namespace RCC.Commands
{
    public class GuLeaveOfficerTitle : CommandBase
    {
        public override string CmdName => "GULeaveOfficerTitle";

        public override string CmdUsage => "";

        public override string CmdDesc => "abandon officer title";

        public override string Run(RyzomClient handler, string command, Dictionary<string, object> localVars)
        {
            var args = GetArgs(command);

            if (args.Length != 0) return "";

            const string msgName = "GUILD:ABANDON_OFFICER_TITLE";
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