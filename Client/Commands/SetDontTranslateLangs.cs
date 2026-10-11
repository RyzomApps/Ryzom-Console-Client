using API;
using API.Commands;
using System;
using System.Collections.Generic;

namespace Client.Commands
{
    public class SetDontTranslateLangs : CommandBase
    {

        public override CommandCategory CmdCategory => CommandCategory.Chat;
        public override string CmdName => "SetDontTranslateLangs";

        public override string CmdUsage => "<langs>";

        public override string CmdDesc => "Set the language channels whose content should NOT be auto-translated. Pass the language codes as a single string, e.g. 'frde' to skip translation of the French and German channels. Pass an empty string to translate everything.";

        public override bool Run(IClient handler, string command, out string responseMsg, Dictionary<string, object> localVars)
        {
            responseMsg = "";
            if (handler is not RyzomClient ryzomClient)
                throw new Exception("Command handler is not a Ryzom client.");

            var args = GetArgs(command);

            // Check parameters and perform admin command
            if (args.Length == 1)
                return ryzomClient.PerformInternalCommand($"a setDontTranslateLangs {args[0]}", out responseMsg);

            responseMsg = "Wrong argument count in the command.";
            return false;
        }
    }
}
