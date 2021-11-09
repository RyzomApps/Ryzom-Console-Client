using RCC.Commands.Internal;
using RCC.Network;
using System.Collections.Generic;

namespace RCC.Commands
{
    public class OutpostChooseSide : CommandBase
    {
        public override string CmdName => "outpostChooseSide";
        public override string CmdUsage => "[<side 0=Owner 1=Attacker>]";
        public override string CmdDesc => "Set the player as 'away from keyboard'";

        public override string Run(RyzomClient handler, string command, Dictionary<string, object> localVars)
        {
            var bNeutral = false; // neutral state

            var args = GetArgs(command);

            if (args.Length != 1)
                return "";

            var pvpSide = int.Parse(args[0]);

            string msgName = "OUTPOST:SIDE_CHOSEN";
            BitMemoryStream out2 = new BitMemoryStream();
            if (handler.GetNetworkManager().GetMessageHeaderManager().PushNameToStream(msgName, out2))
            {
                out2.Serial(ref bNeutral);
                byte sideAsInt = (byte)pvpSide;
                out2.Serial(ref sideAsInt);
                handler.GetNetworkManager().Push(out2);
            }
            else
                handler.GetLogger().Warn($"Unknown message named '{msgName}'.");

            return "";
        }

        public override IEnumerable<string> GetCmdAliases()
        {
            return new string[] { };
        }
    }
}