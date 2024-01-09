using System;
using System.Collections.Generic;
using API;
using API.Commands;

namespace Client.Commands
{
    public class Dodge : CommandBase
    {
        public override string CmdName => "dodge";

        public override string CmdUsage => "";

        public override string CmdDesc => "Set the defense mode to dodge";

        public override string Run(IClient handler, string command, Dictionary<string, object> localVars)
        {
            if (!(handler is RyzomClient ryzomClient))
                throw new Exception("Command handler is not a Ryzom client.");

            ryzomClient.GetNetworkManager().SendMsgToServer("COMBAT:DODGE");

            return "";
        }

        public override IEnumerable<string> GetCmdAliases()
        {
            return new string[] { };
        }
    }
}