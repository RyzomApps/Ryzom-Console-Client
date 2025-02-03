using System;
using System.Collections.Generic;
using API;
using API.Commands;

namespace Client.Commands
{
    public class PhraseSelectMemory : CommandBase
    {
        public override string CmdName => "PhraseSelectMemory";

        public override string CmdUsage => "<value>";

        public override string CmdDesc => "Only one memory line is displayed in the Memory DB. if -1, erased.";

        public override string Run(IClient handler, string command, Dictionary<string, object> localVars)
        {
            if (handler is not RyzomClient ryzomClient)
                throw new Exception("Command handler is not a Ryzom client.");

            var args = GetArgs(command);

            if (args.Length != 1)
            {
                handler.GetLogger().Warn("Please specify an outpost sheet ID.");
                return "";
            }

            if (!int.TryParse(args[0], out var val))
            {
                handler.GetLogger().Warn("Expression doesn't evaluate to a numerical value.");
            }
            else
            {
                var pPm = ryzomClient.GetPhraseManager();

                // first half of memorized stanza sets - MEM_SET_TYPES::NumMemories / 2 - 1
                val = Math.Max(0, Math.Min(val, 10));
                pPm.SelectMemoryLineDb(val);
            }

            return "";
        }

        public override IEnumerable<string> GetCmdAliases()
        {
            return [];
        }
    }
}