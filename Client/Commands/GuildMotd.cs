using System.Collections.Generic;
using Client.Commands.Internal;
using Client.Network;

namespace Client.Commands
{
    public class GuildMotd : CommandBase
    {
        public override string CmdName => "guildmotd";

        public override string CmdUsage => "<msg of the day>";

        public override string CmdDesc => "Set or see the guild message of the day";

        public override string Run(RyzomClient handler, string command, Dictionary<string, object> localVars)
        {
            var args = GetArgs(command);

            var out2 = new BitMemoryStream();

            if (!handler.GetNetworkManager().GetMessageHeaderManager().PushNameToStream("COMMAND:GUILDMOTD", out2)) return "";

            var gmotd = "";

            if (args.Length != 0) gmotd = args[0];

            for (uint i = 1; i < args.Length; ++i)
            {
                gmotd += " ";
                gmotd += args[i];
            }

            out2.Serial(ref gmotd);
            handler.GetNetworkManager().Push(out2);

            return "";
        }

        public override IEnumerable<string> GetCmdAliases()
        {
            return new string[] { };
        }
    }
}