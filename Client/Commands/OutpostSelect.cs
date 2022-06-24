using System;
using System.Collections.Generic;
using API;
using API.Commands;
using Client.Network;
using Client.Stream;

namespace Client.Commands
{
    public class OutpostSelect : CommandBase
    {
        public override string CmdName => "OutpostSelect";

        public override string CmdUsage => "<outpostSheetId>";

        public override string CmdDesc => "Select an outpost to be displayed in the outpost window.";

        public override string Run(IClient handler, string command, Dictionary<string, object> localVars)
        {
            if (!(handler is RyzomClient ryzomClient))
                throw new Exception("Command handler is not a Ryzom client.");

            var args = GetArgs(command);

            if (args.Length != 1)
            {
                handler.GetLogger().Warn($"Please specify an outpostSheetId.");
                return "";
            }

            const string msgName = "OUTPOST:SELECT";
            var out2 = new BitMemoryStream();

            if (ryzomClient.GetNetworkManager().GetMessageHeaderManager().PushNameToStream(msgName, out2))
            {
                var outpostSheet = uint.Parse(args[0]); // can fail but who cares xD

                out2.Serial(ref outpostSheet);

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