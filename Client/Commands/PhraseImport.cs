using System;
using System.Collections.Generic;
using API;
using API.Commands;
using Client.Phrase;

namespace Client.Commands
{
    public class PhraseImport : CommandBase
    {
        public override string CmdName => "PhraseImport";

        public override string CmdUsage => "";

        public override string CmdDesc => "Import a phrase from a file.";

        public override string Run(IClient handler, string command, Dictionary<string, object> localVars)
        {
            if (!(handler is RyzomClient ryzomClient))
                throw new Exception("Command handler is not a Ryzom client.");

            // TODO: this is just a stub

            // forget 10 1
            //ryzomClient.GetPhraseManager().SendForgetToServer(9, 0);

            // learn 10 1
            for (uint i = 0; i < 100; i++)
            {
                if (ryzomClient.GetPhraseManager().GetPhrase(i) == PhraseCom.EmptyPhrase)
                    continue;

                for (uint j = 0; j < 20; j++)
                {
                    ryzomClient.GetPhraseManager().SendLearnToServer(i);
                    ryzomClient.GetPhraseManager().SendMemorizeToServer(9, j, i);
                }

                break;
            }

            return "";
        }

        public override IEnumerable<string> GetCmdAliases()
        {
            return new string[] { };
        }
    }
}