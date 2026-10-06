using API;
using API.Commands;
using Client.Stream;
using System;
using System.Collections.Generic;

namespace Client.Commands
{
    public class Afk : CommandBase
    {
        public override string CmdName => "afk";

        public override string CmdUsage => "[on|off|true|false|0|1] [customText]";

        public override string CmdDesc => "Set the player as 'away from keyboard' (toggles if no state is given)";

        /// <summary>Afk state as a parameter, persisted across runs. Starts with afk off.</summary>
        private bool _afk = false;

        public override bool Run(IClient handler, string command, out string responseMsg, Dictionary<string, object> localVars)
        {
            responseMsg = "";
            if (handler is not RyzomClient ryzomClient)
                throw new Exception("Command handler is not a Ryzom client.");

            var args = GetArgs(command);

            // optional state parameter: on|off|true|false|1|0, otherwise toggle
            if (args.Length > 0)
            {
                var state = args[0].ToLowerInvariant();
                switch (state)
                {
                    case "on":
                    case "true":
                    case "1":
                        _afk = true;
                        args = args[1..];
                        break;
                    case "off":
                    case "false":
                    case "0":
                        _afk = false;
                        args = args[1..];
                        break;
                    default:
                        _afk = !_afk;
                        break;
                }
            }
            else
            {
                _afk = !_afk;
            }

            var b = _afk;

            var customText = "";
            if (args.Length != 0) customText = string.Join(" ", args);

            // send afk state
            var msgName = "COMMAND:AFK";
            var out2 = new BitMemoryStream();

            if (ryzomClient.GetNetworkManager().GetMessageHeaderManager().PushNameToStream(msgName, out2))
            {
                out2.Serial(ref b);
                ryzomClient.GetNetworkManager().Push(out2);
            }
            else
            {
                responseMsg = $"Unknown message named '{msgName}'.";
                return false;
            }

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
                responseMsg = $"Unknown message named '{msgName}'.";
                return false;
            }

            responseMsg = $"AFK state: {(_afk ? "on" : "off")}" + (customText.Length > 0 ? $" (\"{customText}\")" : "");
            return true;
        }
    }
}
