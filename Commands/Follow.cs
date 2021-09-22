using System.Collections.Generic;
using RCC.Commands.Internal;
using RCC.Network;

namespace RCC.Commands
{
    public class Follow : CommandBase
    {
        public override string CmdName => "follow";
        public override string CmdUsage => "";
        public override string CmdDesc => "Follow the target";

        public override string Run(RyzomClient handler, string command, Dictionary<string, object> localVars)
        {
            // Check parameters.
            if (!HasArg(command))
            {
                // Create the message and send.
                const string msgName = "TARGET:FOLLOW";
                BitMemoryStream out2 = new BitMemoryStream();
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