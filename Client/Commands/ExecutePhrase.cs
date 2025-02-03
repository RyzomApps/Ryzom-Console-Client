using System;
using System.Collections.Generic;
using API;
using API.Commands;

namespace Client.Commands
{
    public class ExecutePhrase : CommandBase
    {
        public override string CmdName => "executePhrase";

        public override string CmdUsage => "<memoryId> <slotId> [cyclic]";

        public override string CmdDesc => "Command to send the execution message for a phrase to the server.";

        public override string Run(IClient handler, string command, Dictionary<string, object> localVars)
        {
            if (handler is not RyzomClient ryzomClient)
                throw new Exception("Command handler is not a Ryzom client.");

            var args = GetArgs(command);

            if (args.Length != 2 && args.Length != 3)
                return "Please specify two or three arguments.";

            var cyclic = false;

            var worked = uint.TryParse(args[0], out var memoryLine);
            worked &= uint.TryParse(args[1], out var memorySlot);

            if (args.Length == 3)
                worked &= bool.TryParse(args[2], out cyclic);

            if (!worked)
                return "One of the arguments could not be parsed.";

            // send msg
            ryzomClient.GetPhraseManager().SendExecuteToServer(memoryLine, memorySlot, cyclic);

            return "";
        }

        public override IEnumerable<string> GetCmdAliases()
        {
            return [];
        }
    }
}