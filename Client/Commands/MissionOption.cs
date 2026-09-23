using API;
using API.Commands;
using Client.Stream;
using System;
using System.Collections.Generic;

namespace Client.Commands
{
    /// <summary>
    /// GCM Mission option
    /// </summary>
    public class MissionOption : CommandBase
    {
        public override string CmdName => "missionOption";

        public override string CmdUsage => "<id>";

        public override string CmdDesc => "The user completed the mission, with no gift required.";

        public override bool Run(IClient handler, string command, out string responseMsg, Dictionary<string, object> localVars)
        {
            responseMsg = "";
            if (handler is not RyzomClient ryzomClient)
                throw new Exception("Command handler is not a Ryzom client.");

            var args = GetArgs(command);

            if (!(args.Length == 1 && int.TryParse(args[0], out var intId)))
            {
                responseMsg = "Wrong argument count or argument could not be parsed.";
                return false;
            }

            const string msgName = "BOTCHAT:CONTINUE_MISSION";
            var out2 = new BitMemoryStream();

            if (ryzomClient.GetNetworkManager().GetMessageHeaderManager().PushNameToStream(msgName, out2))
            {
                var id = (byte)intId; // can fail but who cares xD
                out2.Serial(ref id);

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