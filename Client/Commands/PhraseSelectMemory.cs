using API;
using API.Commands;
using System;
using System.Collections.Generic;

namespace Client.Commands
{
    public class PhraseSelectMemory : CommandBase
    {
        public override string CmdName => "PhraseSelectMemory";

        public override string CmdUsage => "<value>";

        public override string CmdDesc => "Only one memory line is displayed in the Memory DB. if -1, erased.";

        public override bool Run(IClient handler, string command, out string responseMsg, Dictionary<string, object> localVars)
        {
            responseMsg = "";
            if (handler is not RyzomClient ryzomClient)
                throw new Exception("Command handler is not a Ryzom client.");

            var args = GetArgs(command);

            if (args.Length != 1)
            {
                responseMsg = "Please specify an outpost sheet ID.";
                return false;
            }

            if (!int.TryParse(args[0], out var val))
            {
                responseMsg = "Expression doesn't evaluate to a numerical value.";
                return false;
            }

            var pPm = ryzomClient.GetPhraseManager();

            // first half of memorized stanza sets - MEM_SET_TYPES::NumMemories / 2 - 1
            val = Math.Max(0, Math.Min(val, 10));
            pPm.SelectMemoryLineDb(val);

            return true;
        }
    }
}