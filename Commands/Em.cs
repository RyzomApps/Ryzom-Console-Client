using System.Collections.Generic;
using RCC.Helper;
using RCC.Messages;
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
            BitMemoryStream out2 = new BitMemoryStream();
            if (GenericMessageHeaderManager.PushNameToStream(msgName, out2))
            {
                emotePhrase = "&EMT&" + "Ich" + " " + emotePhrase; // TODO: use the real name here xD

                out2.Serial(ref behavToSend);
                out2.Serial(ref emotePhrase);
                NetworkManager.Push(out2);
            }
            else
                RyzomClient.Log?.Warn($"Unknown message named '{msgName}'.");

            return "";
        }

        public override IEnumerable<string> getCMDAliases()
        {
            return new string[] { };
        }
    }
}