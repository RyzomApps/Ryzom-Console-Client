using System.Collections.Generic;
using RCC.Commands.Internal;
using RCC.Network;

namespace RCC.Commands
{
    public class GuildCreate : CommandBase
    {
        public override string CmdName => "GuildCreate";

        public override string CmdUsage => "<name> <icon> <description>";

        public override string CmdDesc => "client wants to create a guild (name of new guild, guild icon descriptor, description of the guild)";

        public override string Run(RyzomClient handler, string command, Dictionary<string, object> localVars)
        {
            var args = GetArgs(command);

            if (args.Length != 1) return ""; // TODO: guild create

            const string msgName = "GUILD:CREATE";
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