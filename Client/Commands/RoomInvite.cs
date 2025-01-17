using System;
using System.Collections.Generic;
using API;
using API.Commands;

namespace Client.Commands
{
    public class RoomInvite : CommandBase
    {
        public override string CmdName => "RoomInvite";

        public override string CmdUsage => "<name>";

        public override string CmdDesc => "Invite a friend in your apartment.";

        public override string Run(IClient handler, string command, Dictionary<string, object> localVars)
        {
            if (!(handler is RyzomClient ryzomClient))
                throw new Exception("Command handler is not a Ryzom client.");

            var args = GetArgs(command);

            // Check parameters
            if (args.Length != 1)
                return "Wrong argument count in the command.";

            // RoomInvite - Perform admin command
            ryzomClient.PerformInternalCommand($"a roomInvite {args[0]}", out var response);
            return response;
        }

        public override IEnumerable<string> GetCmdAliases()
        {
            return new[] { "" };
        }
    }
}