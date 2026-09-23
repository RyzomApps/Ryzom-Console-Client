using API;
using API.Commands;
using Client.Stream;
using System;
using System.Collections.Generic;

namespace Client.Commands
{
    public class OutpostSelect : CommandBase
    {
        public override string CmdName => "OutpostSelect";

        public override string CmdUsage => "<outpostSheetId>";

        public override string CmdDesc => "Select an outpost to be displayed in the outpost window.";

        public override bool Run(IClient handler, string command, out string responseMsg, Dictionary<string, object> localVars)
        {
            responseMsg = "";
            if (handler is not RyzomClient ryzomClient)
                throw new Exception("Command handler is not a Ryzom client.");

            var args = GetArgs(command);

            if (args.Length != 1)
            {
                responseMsg = "Please specify an outpost sheet ID.";
                return false;
            }

            const string msgName = "OUTPOST:SELECT";
            var out2 = new BitMemoryStream();

            if (ryzomClient.GetNetworkManager().GetMessageHeaderManager().PushNameToStream(msgName, out2))
            {
                if (!uint.TryParse(args[0], out var outpostSheet))
                {
                    responseMsg = "Outpost sheed ID is not a valid unsigned Integer.";
                    return false;
                }

                out2.Serial(ref outpostSheet);

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