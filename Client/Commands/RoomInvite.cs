using API;
using API.Commands;
using System;
using System.Collections.Generic;

namespace Client.Commands
{
    public class RoomInvite : CommandBase
    {
        public override string CmdName => "RoomInvite";

        public override string CmdUsage => "<name>";

        public override string CmdDesc => "Invite a friend in your apartment.";

        public override bool Run(IClient handler, string command, out string responseMsg, Dictionary<string, object> localVars)
        {
            responseMsg = "";
            if (handler is not RyzomClient ryzomClient)
                throw new Exception("Command handler is not a Ryzom client.");

            var args = GetArgs(command);

            // Check parameters and perform admin command
            if (args.Length == 1)
                return ryzomClient.PerformInternalCommand($"a roomInvite {args[0]}", out responseMsg);

            responseMsg = "Wrong argument count in the command.";
            return false;
        }
    }
}