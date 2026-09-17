using System;
using System.Collections.Generic;
using API;
using API.Commands;
using Client.Stream;

namespace Client.Commands
{
    /// <summary>
    ///  "Roll a dice and say the result around"
    /// </summary>
    public class Random : CommandBase
    {
        public override string CmdName => "random";

        public override string CmdUsage => "[min] <max>";

        public override string CmdDesc => "Roll a dice and say the result around";

        public override bool Run(IClient handler, string command, out string responseMsg, Dictionary<string, object> localVars)
        {
            responseMsg = "";
            if (handler is not RyzomClient ryzomClient)
                throw new Exception("Command handler is not a Ryzom client.");

            var args = GetArgs(command);

            if (args.Length == 0 || args.Length > 2 || !short.TryParse(args[0], out var max))
            {
                responseMsg = $"Usage: {CmdUsage}";
                return false;
            }

            short min = 1;

            if (args.Length > 1 && !short.TryParse(args[1], out min))
            {
                responseMsg = $"Usage: {CmdUsage}";
                return true;
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
                responseMsg = $"Unknown message named '{msgName}'.";
                return false;
            }

            return true;
        }
    }
}