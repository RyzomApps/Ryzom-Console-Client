using RCC.Commands.Internal;
using RCC.Network;
using System.Collections.Generic;

namespace RCC.Commands
{
    public class Afk : CommandBase
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
            if (handler.GetNetworkManager().GetMessageHeaderManager().PushNameToStream(msgName, out2))
            {
                out2.Serial(ref b);
                handler.GetNetworkManager().Push(out2);
            }
            else
                RyzomClient.GetInstance().GetLogger().Warn($"Unknown message named '{msgName}'.");

            // custom afk txt
            BitMemoryStream outTxt = new BitMemoryStream();
            msgName = "STRING:AFK_TXT";
            if (handler.GetNetworkManager().GetMessageHeaderManager().PushNameToStream(msgName, outTxt))
            {
                outTxt.Serial(ref customText);
                handler.GetNetworkManager().Push(outTxt);
            }
            else
            {
                RyzomClient.GetInstance().GetLogger().Warn($"Unknown message named '{msgName}'.");
            }

            return "";
        }

        public override IEnumerable<string> getCMDAliases()
        {
            return new string[] { };
        }
    }
}