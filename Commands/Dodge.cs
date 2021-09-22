using System.Collections.Generic;
using RCC.Commands.Internal;
using RCC.Messages;

namespace RCC.Commands
{
    public class Dodge : CommandBase
    {
        public override string CmdName => "dodge";
        public override string CmdUsage => "";
        public override string CmdDesc => "";

        public override string Run(RyzomClient handler, string command, Dictionary<string, object> localVars)
        {
            GenericMessageHeaderManager.SendMsgToServer("COMBAT:DODGE");

            return "";
        }

        public override IEnumerable<string> getCMDAliases()
        {
            return new string[] { };
        }
    }
}