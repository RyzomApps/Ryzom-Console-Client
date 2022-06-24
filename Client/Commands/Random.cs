using System;
using System.Collections.Generic;
using API;
using API.Commands;
using Client.Network;
using Client.Stream;

namespace Client.Commands
{
    /// <summary>
    ///  "Roll a dice and say the result around"
    /// </summary>
    public class Random : CommandBase
    {
        public override string CmdName => "random";

        public override string CmdUsage => "/random [<min>] <max>";

        public override string CmdDesc => "Roll a dice and say the result around";

        public override string Run(IClient handler, string command, Dictionary<string, object> localVars)
        {
            if (!(handler is RyzomClient ryzomClient))
                throw new Exception("Command handler is not a Ryzom client.");

            var args = GetArgs(command);

            if (args.Length == 0 || args.Length > 2)
                return "Usage: " + CmdUsage;

            if (!short.TryParse(args[0], out var max))
            {
                return "Usage: " + CmdUsage;
            }

            short min = 1;

            if (args.Length > 1)
                if (!short.TryParse(args[1], out min))
                {
                    return "Usage: " + CmdUsage;
                }

            if (min > max)
                (min, max) = (max, min);

            const string msgName = "COMMAND:RANDOM";

            var @out = new BitMemoryStream();

            if (ryzomClient.GetNetworkManager().GetMessageHeaderManager().PushNameToStream(msgName, @out))
            {
                @out.Serial(ref min);
                @out.Serial(ref max);
                ryzomClient.GetNetworkManager().Push(@out);
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