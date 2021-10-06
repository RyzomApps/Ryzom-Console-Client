using System.Collections.Generic;
using RCC.Commands.Internal;
using RCC.Network;

namespace RCC.Commands
{
    public class Sit : CommandBase
    {
        public override string CmdName => "sit";
        public override string CmdUsage => "";
        public override string CmdDesc => "client send to the server the sitting state";

        public override string Run(RyzomClient handler, string command, Dictionary<string, object> localVars)
        {
            bool s = true; // sit state

            // send afk state
            string msgName = "COMMAND:SIT";
            BitMemoryStream out2 = new BitMemoryStream();
            if (handler.GetNetworkManager().GetMessageHeaderManager().PushNameToStream(msgName, out2))
            {
                out2.Serial(ref s);
                handler.GetNetworkManager().Push(out2);
            }
            else
                handler.GetLogger().Warn($"Unknown message named '{msgName}'.");

            return "";
        }

        public override IEnumerable<string> GetCmdAliases()
        {
            return new string[] { };
        }
    }
}