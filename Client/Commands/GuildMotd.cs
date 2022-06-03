using System;
using System.Collections.Generic;
using API;
using API.Commands;
using Client.Network;

namespace Client.Commands
{
    public class GuildMotd : CommandBase
    {
        public override string CmdName => "guildmotd";

        public override string CmdUsage => "<msg of the day>";

        public override string CmdDesc => "Set or see the guild message of the day";

        public override string Run(IClient handler, string command, Dictionary<string, object> localVars)
        {
            if (!(handler is RyzomClient ryzomClient))
                throw new Exception("Command handler is not a Ryzom client.");

            var args = GetArgs(command);

            const string msgName = "COMMAND:GUILDMOTD";
            var out2 = new BitMemoryStream();

            if (!ryzomClient.GetNetworkManager().GetMessageHeaderManager().PushNameToStream("COMMAND:GUILDMOTD", out2))
                return $"Unknown message named '{msgName}'.";

            var gmotd = "";

            if (args.Length != 0) gmotd = args[0];

            for (uint i = 1; i < args.Length; ++i)
            {
                gmotd += " ";
                gmotd += args[i];
            }

            out2.Serial(ref gmotd);
            ryzomClient.GetNetworkManager().Push(out2);

            return "";
        }

        public override IEnumerable<string> GetCmdAliases()
        {
            return new string[] { };
        }
    }
}