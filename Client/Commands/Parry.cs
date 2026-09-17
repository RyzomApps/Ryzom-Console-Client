using System;
using System.Collections.Generic;
using API;
using API.Commands;

namespace Client.Commands
{
    public class Parry : CommandBase
    {
        public override string CmdName => "parry";

        public override string CmdUsage => "";

        public override string CmdDesc => "Set the defense mode to parry";

        public override bool Run(IClient handler, string command, out string responseMsg, Dictionary<string, object> localVars)
        {
            responseMsg = "";
            if (handler is not RyzomClient ryzomClient)
                throw new Exception("Command handler is not a Ryzom client.");

            ryzomClient.GetNetworkManager().SendMsgToServer("COMBAT:PARRY");

            responseMsg = "§eYou will try to parry melee attacks.";
            return true;
        }
    }
}