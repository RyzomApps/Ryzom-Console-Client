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

        public override string Run(IClient handler, string command, Dictionary<string, object> localVars)
        {
            if (!(handler is RyzomClient ryzomClient))
                throw new Exception("Command handler is not a Ryzom client.");

            ryzomClient.GetNetworkManager().SendMsgToServer("COMBAT:PARRY");

            handler.GetLogger().Info("§eYou will try to parry melee attacks.");

            return "";
        }

        public override IEnumerable<string> GetCmdAliases()
        {
            return new string[] { };
        }
    }
}