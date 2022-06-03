using System;
using System.Collections.Generic;
using API;
using API.Commands;
using Client.Network;

namespace Client.Commands
{
    public class AskRespawn : CommandBase
    {
        public override string CmdName => "AskRespawn";

        public override string CmdUsage => "<index>";

        public override string CmdDesc => "Client wants to respawn somewhere (index of the respawn location wanted)";

        public override IEnumerable<string> GetCmdAliases() { return new[] { "respawn" }; }

        public override string Run(IClient handler, string command, Dictionary<string, object> localVars)
        {
            if (!(handler is RyzomClient ryzomClient))
                throw new Exception("Command handler is not a Ryzom client.");

            var args = GetArgs(command);

            if (args.Length != 1)
                return "";

            // send command
            const string msgName = "DEATH:ASK_RESPAWN";

            var index = ushort.Parse(args[0]);
            var out2 = new BitMemoryStream();

            if (ryzomClient.GetNetworkManager().GetMessageHeaderManager().PushNameToStream(msgName, out2))
            {
                out2.Serial(ref index);
                ryzomClient.GetNetworkManager().Push(out2);
            }
            else
                return $"Unknown message named '{msgName}'.";

            return "";
        }
    }
}