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

        public override bool Run(IClient handler, string command, out string responseMsg, Dictionary<string, object> localVars)
        {
            responseMsg = "";
            if (handler is not RyzomClient ryzomClient)
                throw new Exception("Command handler is not a Ryzom client.");

            var args = GetArgs(command);

            if (args.Length != 2 && args.Length != 3)
            {
                responseMsg = "Please specify two or three arguments.";
                return false;
            }

            var cyclic = false;

            var worked = uint.TryParse(args[0], out var memoryLine);
            worked &= uint.TryParse(args[1], out var memorySlot);

            if (args.Length == 3)
                worked &= bool.TryParse(args[2], out cyclic);

            if (!worked)
            {
                responseMsg = "One of the arguments could not be parsed.";
                return false;
            }

            // send msg
            ryzomClient.GetPhraseManager().SendExecuteToServer(memoryLine, memorySlot, cyclic);

            return true;
        }
    }
}