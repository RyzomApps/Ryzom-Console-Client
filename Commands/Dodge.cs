using System.Collections.Generic;
using RCC.Commands.Internal;

namespace RCC.Commands
{
    public class Dodge : CommandBase
    {
        public override string CmdName => "dodge";
        public override string CmdUsage => "";
        public override string CmdDesc => "";

        public override string Run(RyzomClient handler, string command, Dictionary<string, object> localVars)
        {
            handler.GetNetworkManager().SendMsgToServer("COMBAT:DODGE");

            return "";
        }

        public override IEnumerable<string> getCMDAliases()
        {
            return new string[] { };
        }
    }
}