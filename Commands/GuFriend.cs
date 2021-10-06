using System.Collections.Generic;
using RCC.Commands.Internal;
using RCC.Network;

namespace RCC.Commands
{
    public class GuFriend : CommandBase
    {
        public override string CmdName => "GUFriend";

        public override string CmdUsage => "<player name>";

        public override string CmdDesc => "invite a player to become a friend of the guild";

        public override string Run(RyzomClient handler, string command, Dictionary<string, object> localVars)
        {
            var args = GetArgs(command);

            if (args.Length != 1) return "";

            const string msgName = "GUILD:FRIEND_INVITATION";
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