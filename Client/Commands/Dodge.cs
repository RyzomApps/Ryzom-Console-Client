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

        public override bool Run(IClient handler, string command, out string responseMsg, Dictionary<string, object> localVars)
        {
            if (handler is not RyzomClient ryzomClient)
                throw new Exception("Command handler is not a Ryzom client.");

            ryzomClient.GetNetworkManager().SendMsgToServer("COMBAT:DODGE");

            responseMsg = "§eYou will try to dodge melee attacks.";

            return true;
        }
    }
}