using System.Collections.Generic;
using Client.Commands.Internal;
using Client.Network;

namespace Client.Commands
{
    public class NoFollow : CommandBase
    {
        public override string CmdName => "noFollow";
        public override string CmdUsage => "";
        public override string CmdDesc => "Stop following the target";

        public override string Run(RyzomClient handler, string command, Dictionary<string, object> localVars)
        {
            // Check parameters.
            if (HasArg(command)) return "";

            // Create the message and send.
            const string msgName = "TARGET:NO_FOLLOW";
            var out2 = new BitMemoryStream();
                
            if (handler.GetNetworkManager().GetMessageHeaderManager().PushNameToStream(msgName, out2))
            {
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