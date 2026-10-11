using API;
using API.Commands;
using System;
using System.Collections.Generic;

namespace Client.Commands
{
    public class OutpostUnselect : CommandBase
    {

        public override CommandCategory CmdCategory => CommandCategory.Debug;
        public override string CmdName => "OutpostUnselect";

        public override string CmdUsage => "";

        public override string CmdDesc => "Deselect the selected outpost; stops the outpost information updates in the database.";

        public override bool Run(IClient handler, string command, out string responseMsg, Dictionary<string, object> localVars)
        {
            responseMsg = "";
            if (handler is not RyzomClient ryzomClient)
                throw new Exception("Command handler is not a Ryzom client.");

            var args = GetArgs(command);

            if (args.Length != 0)
            {
                responseMsg = "This command has no arguments.";
                return false;
            }

            // Send the message to the server
            ryzomClient.GetNetworkManager().SendMsgToServer("OUTPOST:UNSELECT");

            return true;
        }
    }
}
