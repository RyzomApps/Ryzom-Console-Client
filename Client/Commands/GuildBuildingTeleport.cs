using System;
using System.Collections.Generic;
using API;
using API.Commands;
using Client.Stream;

namespace Client.Commands
{
    public class GuildBuildingTeleport : CommandBase
    {
        public override string CmdName => "GuildBuildingTeleport";

        public override string CmdUsage => "<buildingIndex>";

        public override string CmdDesc => "Client wants to teleport somewhere in guild flats";

        public override bool Run(IClient handler, string command, out string responseMsg, Dictionary<string, object> localVars)
        {
            responseMsg = "";
            if (handler is not RyzomClient ryzomClient)
                throw new Exception("Command handler is not a Ryzom client.");

            var args = GetArgs(command);

            if (args.Length != 1)
            {
                responseMsg = "Please specify a parameter.";
                return false;
            }

            if (!ushort.TryParse(args[0], out var index))
                return true;

            const string msgName = "GUILD:TELEPORT";
            var out2 = new BitMemoryStream();

            if (!ryzomClient.GetNetworkManager().GetMessageHeaderManager().PushNameToStream(msgName, out2))
            {
                responseMsg = $"Unknown message named '{msgName}'.";
                return false;
            }

            out2.Serial(ref index);
            ryzomClient.GetNetworkManager().Push(out2);

            return true;
        }
    }
}