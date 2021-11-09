using System.Collections.Generic;
using Client.Commands.Internal;
using Client.Network;

namespace Client.Commands
{
    public class Where : CommandBase
    {
        public override string CmdName => "where";
        public override string CmdUsage => "";
        public override string CmdDesc => "Ask information on the position";

        public override string Run(RyzomClient handler, string command, Dictionary<string, object> localVars)
        {
            // Check parameters.
            if (!HasArg(command))
            {
                // Create the message and send.
                const string msgName = "COMMAND:WHERE";
                var out2 = new BitMemoryStream();

                if (handler.GetNetworkManager().GetMessageHeaderManager().PushNameToStream(msgName, out2))
                {
                    handler.GetNetworkManager().Push(out2);
                }
                else
                    handler.GetLogger().Warn($"Unknown message named '{msgName}'.");
            }

            return "";
        }

        public override IEnumerable<string> GetCmdAliases()
        {
            return new string[] { };
        }
    }
}