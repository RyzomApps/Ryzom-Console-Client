using System;
using System.Collections.Generic;
using API;
using API.Commands;
using Client.Stream;

namespace Client.Commands
{
    /// <summary>
    /// create a guild
    /// </summary>
    public class GuildCreate : CommandBase
    {
        public override string CmdName => "GuildCreate";

        public override string CmdUsage => "<name> <icon> <description>";

        public override string CmdDesc =>
            "Client wants to create a guild (name of new guild, guild icon descriptor, description of the guild)";

        public override string Run(IClient handler, string command, Dictionary<string, object> localVars)
        {
            if (!(handler is RyzomClient ryzomClient))
                throw new Exception("Command handler is not a Ryzom client.");

            var args = GetArgs(command);

            if (args.Length != 3)
                return "Wrong argument count in the command.";

            const string msgName = "GUILD:CREATE";
            var out2 = new BitMemoryStream();

            if (!ryzomClient.GetNetworkManager().GetMessageHeaderManager().PushNameToStream(msgName, out2))
                return $"Unknown message named '{msgName}'.";

            var guildName = "Guild"; //args[0];
            //var icon = ulong.Parse(args[1]);
            ulong icon = 110142619738865821;
            var guildDesc = "Name"; //args[2];

            out2.Serial(ref guildName, true);
            out2.Serial(ref icon, 64 * 8);
            out2.Serial(ref guildDesc, true);

            ryzomClient.GetNetworkManager().Push(out2);

            return "";
        }

        public override IEnumerable<string> GetCmdAliases()
        {
            return new string[] { };
        }
    }
}