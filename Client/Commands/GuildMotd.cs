using System;
using System.Collections.Generic;
using API;
using API.Commands;
using Client.Stream;

namespace Client.Commands
{
    public class GuildMotd : CommandBase
    {
        public override string CmdName => "guildmotd";

        public override string CmdUsage => "<message>";

        public override string CmdDesc => "Set or see the guild message of the day";

        public override bool Run(IClient handler, string command, out string responseMsg, Dictionary<string, object> localVars)
        {
            responseMsg = "";
            if (handler is not RyzomClient ryzomClient)
                throw new Exception("Command handler is not a Ryzom client.");

            var args = GetArgs(command);

            const string msgName = "COMMAND:GUILDMOTD";
            var out2 = new BitMemoryStream();

            if (!ryzomClient.GetNetworkManager().GetMessageHeaderManager().PushNameToStream("COMMAND:GUILDMOTD", out2))
            {
                responseMsg = $"Unknown message named '{msgName}'.";
                return false;
            }

            var gmotd = "";

            if (args.Length != 0)
                gmotd = args[0];

            for (uint i = 1; i < args.Length; ++i)
            {
                gmotd += " ";
                gmotd += args[i];
            }

            out2.Serial(ref gmotd);
            ryzomClient.GetNetworkManager().Push(out2);

            return true;
        }
    }
}