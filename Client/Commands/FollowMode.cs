using System;
using System.Collections.Generic;
using API;
using API.Commands;
using Client.Stream;

namespace Client.Commands
{
    public class FollowMode : CommandBase
    {
        public override string CmdName => "followMode";

        public override string CmdUsage => "";

        public override string CmdDesc => "Start the mode for following the target (only for server events)";

        public override string Run(IClient handler, string command, Dictionary<string, object> localVars)
        {
            if (handler is not RyzomClient ryzomClient)
                throw new Exception("Command handler is not a Ryzom client.");

            // Check parameters.
            if (HasArg(command)) return "";

            // Create the message and send.
            const string msgName = "TARGET:FOLLOW";
            var out2 = new BitMemoryStream();

            if (ryzomClient.GetNetworkManager().GetMessageHeaderManager().PushNameToStream(msgName, out2))
                ryzomClient.GetNetworkManager().Push(out2);
            else
                return $"Unknown message named '{msgName}'.";

            return "";
        }

        public override IEnumerable<string> GetCmdAliases()
        {
            return [];
        }
    }
}