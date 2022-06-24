using System;
using System.Collections.Generic;
using API;
using API.Commands;
using Client.Network;
using Client.Stream;

namespace Client.Commands
{
    public class Afk : CommandBase
    {
        public override string CmdName => "afk";

        public override string CmdUsage => "[<custom text>]";

        public override string CmdDesc => "Set the player as 'away from keyboard'";

        public override string Run(IClient handler, string command, Dictionary<string, object> localVars)
        {
            if (!(handler is RyzomClient ryzomClient))
                throw new Exception("Command handler is not a Ryzom client.");

            var b = true; // afk state
            var args = GetArgs(command);

            var customText = "";

            if (args.Length != 0)
            {
                customText = string.Join(" ", args);
            }

            // send afk state
            var msgName = "COMMAND:AFK";
            var out2 = new BitMemoryStream();

            if (ryzomClient.GetNetworkManager().GetMessageHeaderManager().PushNameToStream(msgName, out2))
            {
                out2.Serial(ref b);
                ryzomClient.GetNetworkManager().Push(out2);
            }
            else
                return $"Unknown message named '{msgName}'.";

            // custom afk txt
            var outTxt = new BitMemoryStream();
            msgName = "STRING:AFK_TXT";

            if (ryzomClient.GetNetworkManager().GetMessageHeaderManager().PushNameToStream(msgName, outTxt))
            {
                outTxt.Serial(ref customText);
                ryzomClient.GetNetworkManager().Push(outTxt);
            }
            else
            {
                return $"Unknown message named '{msgName}'.";
            }

            return "";
        }

        public override IEnumerable<string> GetCmdAliases()
        {
            return new string[] { };
        }
    }
}