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

        public override string Run(IClient handler, string command, Dictionary<string, object> localVars)
        {
            if (!(handler is RyzomClient ryzomClient))
                throw new Exception("Command handler is not a Ryzom client.");

            var args = GetArgs(command);

            if (!(args.Length == 1 && int.TryParse(args[0], out var intId)))
            {
                return "Wrong argument count or argument could not be parsed."; ;
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
                return $"Unknown message named '{msgName}'.";
            }

            return "";
        }

        public override IEnumerable<string> GetCmdAliases()
        {
            return new[] { "" };
        }
    }
}