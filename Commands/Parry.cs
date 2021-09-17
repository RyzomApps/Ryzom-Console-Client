using System.Collections.Generic;
using RCC.Messages;

namespace RCC.Commands
{
    public class Parry : Command
    {
        public override string CmdName => "parry";
        public override string CmdUsage => "";
        public override string CmdDesc => "";

        public override string Run(RyzomClient handler, string command, Dictionary<string, object> localVars)
        {
            GenericMessageHeaderManager.SendMsgToServer("COMBAT:PARRY");

            return "";
        }

        public override IEnumerable<string> getCMDAliases()
        {
            return new string[] { };
        }
    }
}