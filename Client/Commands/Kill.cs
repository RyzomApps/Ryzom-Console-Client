using API;
using API.Commands;
using System;
using System.Collections.Generic;

namespace Client.Commands
{
    public class Kill : CommandBase
    {
        public override string CmdName => "selfkill";
        public override string CmdUsage => "";
        public override string CmdDesc => "Kill the player";

        public override bool Run(IClient handler, string command, out string responseMsg, Dictionary<string, object> localVars)
        {
            responseMsg = "";
            if (handler is not RyzomClient ryzomClient)
                throw new Exception("Command handler is not a Ryzom client.");

            ryzomClient.GetNetworkManager().SendMsgToServer("COMMAND:SELFKILL");

            return true;
        }

        public override IEnumerable<string> GetCmdAliases()
        {
            return ["kill"];
        }
    }
}