﻿using System.Collections.Generic;
using RCC.Commands.Internal;
using RCC.Network;

namespace RCC.Commands
{
    public class BuildingTeleport : CommandBase
    {
        public override string CmdName => "buildingTeleport";

        public override string CmdUsage => "building index";

        public override string CmdDesc => "teleport to a building";

        public override string Run(RyzomClient handler, string command, Dictionary<string, object> localVars)
        {
            var args = GetArgs(command);

            if (args.Length != 1) return "";

            if (!ushort.TryParse(args[0], out var index)) return "";

            const string msgName = "GUILD:TELEPORT";
            var out2 = new BitMemoryStream();

            if (handler.GetNetworkManager().GetMessageHeaderManager().PushNameToStream(msgName, out2))
            {
                out2.Serial(ref index);
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