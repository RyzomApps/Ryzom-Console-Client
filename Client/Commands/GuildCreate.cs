using System;
using System.Collections.Generic;
using API;
using API.Commands;
using Client.Network;

namespace Client.Commands
{
    /// <summary>
    /// create a guild
    /// </summary>
    public class GuildCreate : CommandBase
    {
        public override string CmdName => "GuildCreate";

        public override string CmdUsage => "<guild name>";

        public override string CmdDesc => "Client wants to create a guild (name of new guild, guild icon descriptor, description of the guild)";

        public override string Run(IClient handler, string command, Dictionary<string, object> localVars)
        {
            if (!(handler is RyzomClient ryzomClient))
                throw new Exception("Command handler is not a Ryzom client.");

            var args = GetArgs(command);

            if (args.Length != 1) 
                return "Please specify a name."; // TODO: guild create

            const string msgName = "GUILD:CREATE";
            var out2 = new BitMemoryStream();

            if (!ryzomClient.GetNetworkManager().GetMessageHeaderManager().PushNameToStream(msgName, out2)) 
                return $"Unknown message named '{msgName}'.";

            var buf = args[0];
            out2.Serial(ref buf);
            ryzomClient.GetNetworkManager().Push(out2);

            return "";
        }

        public override IEnumerable<string> GetCmdAliases()
        {
            return new string[] { };
        }
    }
}