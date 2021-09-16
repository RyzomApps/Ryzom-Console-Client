using System.Collections.Generic;
using RCC.Helper;
using RCC.Msg;
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
            {   // Create the message and send.
                const string msgName = "TARGET:FOLLOW";
                CBitMemStream out2 = new CBitMemStream();
                if (GenericMsgHeaderMngr.pushNameToStream(msgName, out2))
                {
                    NetworkManager.push(out2);
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