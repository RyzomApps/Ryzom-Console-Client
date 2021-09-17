using System.Collections.Generic;
using RCC.Messages;
using RCC.Network;

namespace RCC.Commands
{
    public class Follow : Command
    {
        public override string CmdName => "follow";
        public override string CmdUsage => "";
        public override string CmdDesc => "Follow the target";

        public override string Run(RyzomClient handler, string command, Dictionary<string, object> localVars)
        {
            // Check parameters.
            if (!hasArg(command))
            {
                // Create the message and send.
                const string msgName = "TARGET:FOLLOW";
                BitMemoryStream out2 = new BitMemoryStream();
                if (GenericMessageHeaderManager.PushNameToStream(msgName, out2))
                {
                    NetworkManager.Push(out2);
                }
                else
                    RyzomClient.Log?.Warn($"Unknown message named '{msgName}'.");
            }

            return "";
        }

        public override IEnumerable<string> getCMDAliases()
        {
            return new string[] { };
        }
    }
}