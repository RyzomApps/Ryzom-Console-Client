using System.Collections.Generic;
using Client.Commands.Internal;
using Client.Network;

namespace Client.Commands
{
    public class GuildRefuseInvitation : CommandBase
    {
        public override string CmdName => "GuildRefuseInvitation";

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