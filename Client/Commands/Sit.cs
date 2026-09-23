using API;
using API.Commands;
using Client.Stream;
using System;
using System.Collections.Generic;

namespace Client.Commands
{
    public class Sit : CommandBase
    {
        public override string CmdName => "sit";

        public override string CmdUsage => "[sitState]";

        public override string CmdDesc => "client send to the server the sitting state";

        public override bool Run(IClient handler, string command, out string responseMsg, Dictionary<string, object> localVars)
        {
            responseMsg = "";
            if (handler is not RyzomClient ryzomClient)
                throw new Exception("Command handler is not a Ryzom client.");

            var s = true; // sit state
            var args = GetArgs(command);

            switch (args.Length)
            {
                case 1 when !bool.TryParse(args[0], out s):
                    responseMsg = "One of the arguments could not be parsed.";
                    return false;

                case > 1:
                    responseMsg = "Please specify zero or one argument.";
                    return false;
            }

            // send AFK state
            const string msgName = "COMMAND:SIT";
            var out2 = new BitMemoryStream();

            if (ryzomClient.GetNetworkManager().GetMessageHeaderManager().PushNameToStream(msgName, out2))
            {
                out2.Serial(ref s);
                ryzomClient.GetNetworkManager().Push(out2);
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