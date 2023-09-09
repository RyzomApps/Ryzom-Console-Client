using System;
using System.Collections.Generic;
using API;
using API.Commands;
using Client.Stream;

namespace Client.Commands
{
    public class OutpostUnselect : CommandBase
    {
        public override string CmdName => "OutpostUnselect";

        public override string CmdUsage => "";

        public override string CmdDesc => "Called when the Outpost State window (the one opened from BotChat) is closed.";

        public override string Run(IClient handler, string command, Dictionary<string, object> localVars)
        {
            if (!(handler is RyzomClient ryzomClient))
                throw new Exception("Command handler is not a Ryzom client.");

            var args = GetArgs(command);

            if (args.Length != 0)
            {
                handler.GetLogger().Warn("This command has no arguments.");
                return "";
            }

            // Send the message to the server
            ryzomClient.GetNetworkManager().SendMsgToServer("OUTPOST:UNSELECT");

            return "";
        }

        public override IEnumerable<string> GetCmdAliases()
        {
            return new string[] { };
        }
    }
}
