using System.Collections.Generic;
using RCC.Helper;
using RCC.Msg;
using RCC.Network;

namespace RCC.Commands
{
    public class Who : Command
    {
        public override string CmdName => "who";
        public override string CmdUsage => "[<options (GM, channel name)>]";
        public override string CmdDesc => "Display all players currently in region";

        public override string Run(RyzomClient handler, string command, Dictionary<string, object> localVars)
        {
            // Check parameters.
            if (getArgs(command).Length > 1)
                return "";

            CBitMemStream out2 = new CBitMemStream();
            if (!GenericMsgHeaderMngr.pushNameToStream("DEBUG:WHO", out2))
            {
                ConsoleIO.WriteLineFormatted("§Unknown message name DEBUG:WHO");
                return "";
            }

            string opt = "";
            if (getArgs(command).Length == 1)
            {
                opt = getArgs(command)[0];
            }

            out2.serial(ref opt);
            NetworkManager.push(out2);
            return "";
        }

        public override IEnumerable<string> getCMDAliases()
        {
            return new[] { "" };
        }
    }
}