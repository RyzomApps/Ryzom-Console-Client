using System;
using System.Collections.Generic;
using API;
using API.Commands;
using Client.Stream;

namespace Client.Commands
{
    public class PvpTag : CommandBase
    {
        public override string CmdName => "PvpTag";

        public override string CmdUsage => "<uint8>";

        public override string CmdDesc => "Set the PVP tag of the player.";

        public override string Run(IClient handler, string command, Dictionary<string, object> localVars)
        {
            if (!(handler is RyzomClient ryzomClient))
                throw new Exception("Command handler is not a Ryzom client.");

            var args = GetArgs(command);

            if (args.Length != 1)
            {
                handler.GetLogger().Warn("Please specify a tag.");
                return "";
            }

            if (!byte.TryParse(args[0], out var tag))
            {
                handler.GetLogger().Warn("Could not parse the tag.");
                return "";
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