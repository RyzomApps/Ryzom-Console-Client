using API;
using API.Commands;
using System;
using System.Collections.Generic;

namespace Client.Commands
{
    /// <summary>
    /// Export all phrases to a file, matching the format used by PhraseImport.
    /// </summary>
    public class PhraseExport : CommandBase
    {
        public override CommandCategory CmdCategory => CommandCategory.Combat;
        public override string CmdName => "PhraseExport";
        public override string CmdUsage => "[filename] [line] [index]";
        public override string CmdDesc => "Command to export all phrases to a file. Optionally filter by memory line and/or slot index.";

        public override bool Run(IClient handler, string command, out string responseMsg, Dictionary<string, object> localVars)
        {
            responseMsg = "";
            if (handler is not RyzomClient ryzomClient)
                throw new Exception("Command handler is not a Ryzom client.");

            var args = GetArgs(command);

            if (args.Length > 3)
            {
                responseMsg = "Please specify less parameters.";
                return false;
            }

            var fileName = args[0];
            uint? memoryLine = null;
            if (args.Length >= 2)
            {
                if (!uint.TryParse(args[1], out var line))
                {
                    responseMsg = "Invalid memory line specified. It must be a number.";
                    return false;
                }

                memoryLine = line;
            }

            uint? memoryIndex = null;
            if (args.Length >= 3)
            {
                if (!uint.TryParse(args[2], out var index))
                {
                    responseMsg = "Invalid index specified. It must be a number.";
                    return false;
                }

                memoryIndex = index;
            }

            ryzomClient.GetPhraseManager().Write($"{fileName}_phrases.rec", memoryLine, memoryIndex);

            return true;
        }

        public override IEnumerable<string> GetCmdAliases()
        {
            return ["ExportPhrase"];
        }
    }
}
