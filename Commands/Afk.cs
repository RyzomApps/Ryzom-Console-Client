using System.Collections.Generic;
using RCC.Helper;
using RCC.Messages;
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
            BitMemoryStream out2 = new BitMemoryStream();
            if (GenericMessageHeaderManager.PushNameToStream(msgName, out2))
            {
                out2.Serial(ref b);
                NetworkManager.Push(out2);
            }
            else
                RyzomClient.Log?.Warn($"Unknown message named '{msgName}'.");

            // custom afk txt
            BitMemoryStream outTxt = new BitMemoryStream();
            msgName = "STRING:AFK_TXT";
            if (GenericMessageHeaderManager.PushNameToStream(msgName, outTxt))
            {
                outTxt.Serial(ref customText);
                NetworkManager.Push(outTxt);
            }
            else
            {
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