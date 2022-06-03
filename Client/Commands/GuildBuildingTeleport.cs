using System;
using System.Collections.Generic;
using API;
using API.Commands;
using Client.Network;

namespace Client.Commands
{
    public class GuildBuildingTeleport : CommandBase
    {
        public override string CmdName => "GuildBuildingTeleport";

        public override string CmdUsage => "building index";

        public override string CmdDesc => "Client wants to teleport somewhere in guild flats";

        public override string Run(IClient handler, string command, Dictionary<string, object> localVars)
        {
            if (!(handler is RyzomClient ryzomClient))
                throw new Exception("Command handler is not a Ryzom client.");

            var args = GetArgs(command);

            if (args.Length != 1) return "";

            if (!ushort.TryParse(args[0], out var index)) return "";

            const string msgName = "GUILD:TELEPORT";
            var out2 = new BitMemoryStream();

            if (!ryzomClient.GetNetworkManager().GetMessageHeaderManager().PushNameToStream(msgName, out2)) 
                return $"Unknown message named '{msgName}'.";

            out2.Serial(ref index);
            ryzomClient.GetNetworkManager().Push(out2);

            return "";
        }

        public override IEnumerable<string> GetCmdAliases()
        {
            return new string[] { };
        }
    }
}