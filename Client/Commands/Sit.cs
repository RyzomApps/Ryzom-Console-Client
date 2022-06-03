using System;
using System.Collections.Generic;
using API;
using API.Commands;
using Client.Network;

namespace Client.Commands
{
    public class Sit : CommandBase
    {
        public override string CmdName => "sit";

        public override string CmdUsage => "<[sit state]>";

        public override string CmdDesc => "client send to the server the sitting state";

        public override string Run(IClient handler, string command, Dictionary<string, object> localVars)
        {
            if (!(handler is RyzomClient ryzomClient))
                throw new Exception("Command handler is not a Ryzom client.");

            var s = true; // sit state
            var args = GetArgs(command);

            if (args.Length == 1)
            {
                if (!bool.TryParse(args[0], out s))
                {
                    return "One of the arguments could not be parsed.";
                }
            }
            else if (args.Length > 1)
            {
                return "Please specify zero or one argument.";
            }

            // send afk state
            const string msgName = "COMMAND:SIT";
            var out2 = new BitMemoryStream();

            if (ryzomClient.GetNetworkManager().GetMessageHeaderManager().PushNameToStream(msgName, out2))
            {
                out2.Serial(ref s);
                ryzomClient.GetNetworkManager().Push(out2);
            }
            else
                return $"Unknown message named '{msgName}'.";

            return "";
        }

        public override IEnumerable<string> GetCmdAliases()
        {
            return new string[] { };
        }
    }
}