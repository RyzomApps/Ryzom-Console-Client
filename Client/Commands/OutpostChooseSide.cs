using System;
using System.Collections.Generic;
using API;
using API.Commands;
using Client.Stream;

namespace Client.Commands
{
    public class OutpostChooseSide : CommandBase
    {
        public override string CmdName => "outpostChooseSide";

        public override string CmdUsage => "<0:Defend|1:Attack|2:Neutral>";

        public override string CmdDesc => "Lets the client chose a side in an outpost war";

        public override bool Run(IClient handler, string command, out string responseMsg, Dictionary<string, object> localVars)
        {
            responseMsg = "";
            if (handler is not RyzomClient ryzomClient)
                throw new Exception("Command handler is not a Ryzom client.");

            var bNeutral = false; // neutral state

            var args = GetArgs(command);

            if (args.Length != 1)
            {
                responseMsg = GetCmdDescTranslated();
                return false;
            }

            var pvpSide = int.Parse(args[0]);

            if (pvpSide is < 0 or > 1)
            {
                pvpSide = 0;
                bNeutral = true;
            }

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
            {
                responseMsg = $"Unknown message named '{msgName}'.";
                return false;
            }

            return true;
        }
    }
}