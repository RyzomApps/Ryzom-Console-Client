using System;
using System.Collections.Generic;
using API;
using API.Commands;
using Client.Network;
using Client.Stream;

namespace Client.Commands
{
    public class OutpostChooseSide : CommandBase
    {
        public override string CmdName => "outpostChooseSide";

        public override string CmdUsage => "[<side 0=Owner 1=Attacker>]";

        public override string CmdDesc => "Lets the client chose a side in an outpost war";

        public override string Run(IClient handler, string command, Dictionary<string, object> localVars)
        {
            if (!(handler is RyzomClient ryzomClient))
                throw new Exception("Command handler is not a Ryzom client.");

            var bNeutral = false; // neutral state

            var args = GetArgs(command);

            if (args.Length != 1)
                return "";

            var pvpSide = int.Parse(args[0]);

            const string msgName = "OUTPOST:SIDE_CHOSEN";
            var out2 = new BitMemoryStream();

            if (ryzomClient.GetNetworkManager().GetMessageHeaderManager().PushNameToStream(msgName, out2))
            {
                out2.Serial(ref bNeutral);
                var sideAsInt = (byte)pvpSide;
                out2.Serial(ref sideAsInt);
                ryzomClient.GetNetworkManager().Push(out2);
            }
            else
                return $"Unknown message named '{msgName}'.";

            return "";
        }

        public override IEnumerable<string> GetCmdAliases()
        {
            return new string[] { };
        }
    }
}