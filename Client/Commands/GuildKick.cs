using System.Collections.Generic;
using RCC.Commands.Internal;
using RCC.Network;

namespace RCC.Commands
{
    public class GuildKick : CommandBase
    {
        public override string CmdName => "GuildKick";

        public override string CmdUsage => "<player name> <counter>";

        public override string CmdDesc => "client (lead,ho,of) wants to kick member specifying its index. Last param is the counter";

        public override string Run(RyzomClient handler, string command, Dictionary<string, object> localVars)
        {
            var args = GetArgs(command);

            if (args.Length != 1) return "";

            const string msgName = "GUILD:KICK_MEMBER"; // TODO GUKick right arguments
            var out2 = new BitMemoryStream();

            if (handler.GetNetworkManager().GetMessageHeaderManager().PushNameToStream(msgName, out2))
            {
                var buf = args[0];
                out2.Serial(ref buf);
                handler.GetNetworkManager().Push(out2);
            }

            return "";
        }

        public override IEnumerable<string> GetCmdAliases()
        {
            return new string[] { };
        }
    }
}