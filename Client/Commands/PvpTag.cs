using API;
using API.Commands;
using Client.Stream;
using System;
using System.Collections.Generic;

namespace Client.Commands
{
    public class PvpTag : CommandBase
    {
        public override string CmdName => "PvpTag";

        public override string CmdUsage => "<uint8>";

        public override string CmdDesc => "Set the PVP tag of the player.";

        public override bool Run(IClient handler, string command, out string responseMsg, Dictionary<string, object> localVars)
        {
            responseMsg = "";
            if (handler is not RyzomClient ryzomClient)
                throw new Exception("Command handler is not a Ryzom client.");

            var args = GetArgs(command);

            if (args.Length != 1)
            {
                responseMsg = "Please specify a tag.";
                return false;
            }

            if (!byte.TryParse(args[0], out var tag))
            {
                responseMsg = "Could not parse the tag.";
                responseMsg = "";
                return false;
            }

            // send tag
            const string msgName = "PVP:PVP_TAG";
            var out2 = new BitMemoryStream();

            if (ryzomClient.GetNetworkManager().GetMessageHeaderManager().PushNameToStream(msgName, out2))
            {
                out2.Serial(ref tag);
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