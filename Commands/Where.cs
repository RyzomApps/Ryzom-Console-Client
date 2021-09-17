using System.Collections.Generic;
using RCC.Helper;
using RCC.Messages;
using RCC.Network;

namespace RCC.Commands
{
    public class Where : Command
    {
        public override string CmdName => "where";
        public override string CmdUsage => "";
        public override string CmdDesc => "Ask information on the position";

        public override string Run(RyzomClient handler, string command, Dictionary<string, object> localVars)
        {
            // Check parameters.
            if (!hasArg(command))
            {
                // Create the message and send.
                const string msgName = "COMMAND:WHERE";
                BitMemoryStream out2 = new BitMemoryStream();
                if (GenericMessageHeaderManager.PushNameToStream(msgName, out2))
                {
                    NetworkManager.Push(out2);
                }
                else
                    ConsoleIO.WriteLineFormatted($"§cUnknown message named '{msgName}'.");
            }

            return "";
        }

        public override IEnumerable<string> getCMDAliases()
        {
            return new string[] { };
        }
    }
}