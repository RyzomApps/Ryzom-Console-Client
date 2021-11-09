using RCC.Commands.Internal;
using RCC.Network;
using System.Collections.Generic;

namespace RCC.Commands
{
    public class AskRespawn : CommandBase
    {
        public override string CmdName => "AskRespawn";
        public override string CmdUsage => "<index>";
        public override string CmdDesc => "client wants to respawn somewhere (index of the respawn location wanted)";
        public override IEnumerable<string> GetCmdAliases() { return new[] { "respawn" }; }

        public override string Run(RyzomClient handler, string command, Dictionary<string, object> localVars)
        {
            var args = GetArgs(command);

            if (args.Length != 1)
                return "";

            // send command
            const string msgName = "DEATH:ASK_RESPAWN";

            var index = ushort.Parse(args[0]);
            var out2 = new BitMemoryStream();

            if (handler.GetNetworkManager().GetMessageHeaderManager().PushNameToStream(msgName, out2))
            {
                out2.Serial(ref index);
                handler.GetNetworkManager().Push(out2);
            }
            else
                handler.GetLogger().Warn($"Unknown message named '{msgName}'.");

            return "";
        }
    }
}