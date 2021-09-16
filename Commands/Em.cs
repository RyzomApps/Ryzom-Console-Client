using System.Collections.Generic;
using RCC.Helper;
using RCC.Msg;
using RCC.Network;

namespace RCC.Commands
{
    // TODO: em command is not working yet
    public class Em : Command
    {
        public override string CmdName => "em";
        public override string CmdUsage => "<emote phrase>";
        public override string CmdDesc => "emote command";

        public override string Run(RyzomClient handler, string command, Dictionary<string, object> localVars)
        {
            var args = getArgs(command);

            if (args.Length < 1)
                return "";

            string emotePhrase = "";
            byte behavToSend = 1; //MBEHAV::IDLE;

            if (args.Length != 0)
            {
                emotePhrase = string.Join(" ", args);
            }

            // Create the message and send.
            const string msgName = "COMMAND:CUSTOM_EMOTE";
            CBitMemStream out2 = new CBitMemStream();
            if (GenericMsgHeaderMngr.pushNameToStream(msgName, out2))
            {
                emotePhrase = "&EMT&" + "Ich" + " " + emotePhrase; // TODO: use the real name here xD

                out2.serial(ref behavToSend);
                out2.serial(ref emotePhrase);
                NetworkManager.push(out2);
            }
            else
                ConsoleIO.WriteLineFormatted($"§cUnknown message named '{msgName}'.");

            return "";
        }

        public override IEnumerable<string> getCMDAliases()
        {
            return new string[] { };
        }
    }
}