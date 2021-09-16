using System.Collections.Generic;
using RCC.Helper;
using RCC.Msg;
using RCC.Network;

namespace RCC.Commands
{
    public class Afk : Command
    {
        public override string CmdName => "afk";
        public override string CmdUsage => "[<custom text>]";
        public override string CmdDesc => "Set the player as 'away from keyboard'";

        public override string Run(RyzomClient handler, string command, Dictionary<string, object> localVars)
        {
            bool b = true; // afk state
            var args = getArgs(command);

            string customText = "";
            if (args.Length != 0)
            {
                customText = string.Join(" ", args);
            }

            // send afk state
            string msgName = "COMMAND:AFK";
            CBitMemStream out2 = new CBitMemStream();
            if (GenericMsgHeaderMngr.pushNameToStream(msgName, out2))
            {
                out2.serial(ref b);
                NetworkManager.push(out2);
            }
            else
                ConsoleIO.WriteLineFormatted($"§cUnknown message named '{msgName}'.");

            // custom afk txt
            CBitMemStream outTxt = new CBitMemStream();
            msgName = "STRING:AFK_TXT";
            if (GenericMsgHeaderMngr.pushNameToStream(msgName, outTxt))
            {
                outTxt.serial(ref customText);
                NetworkManager.push(outTxt);
            }
            else
            {
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