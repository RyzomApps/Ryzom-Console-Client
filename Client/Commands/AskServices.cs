using System.Collections.Generic;
using Client.Commands.Internal;
using Client.Network;

namespace Client.Commands
{
    public class AskServices : CommandBase
    {
        public override string CmdName => "askservices";
        public override string CmdUsage => "";
        public override string CmdDesc => "Ask the server all services up";

        public override string Run(RyzomClient handler, string command, Dictionary<string, object> localVars)
        {
            const string msgName = "DEBUG:SERVICES";

            var out2 = new BitMemoryStream();
            if (handler.GetNetworkManager().GetMessageHeaderManager().PushNameToStream(msgName, out2))
            {
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