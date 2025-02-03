using System;
using System.Collections.Generic;
using API;
using API.Commands;
using Client.Phrase;

namespace Client.Commands
{
    public class PhrasesErase : CommandBase
    {
        public override string CmdName => "PhrasesErase";

        public override string CmdUsage => "[page]";

        public override string CmdDesc => "Erase phrases from the action bar. Specify an ActionBarPage to erase phrases from that page.";

        public override string Run(IClient handler, string command, Dictionary<string, object> localVars)
        {
            if (handler is not RyzomClient ryzomClient)
                throw new Exception("Command handler is not a Ryzom client.");

            var args = GetArgs(command);
            uint? actionBarPage = null;

            if (args.Length == 1)
            {
                if (!uint.TryParse(args[0], out var page))
                    return "Invalid ActionBarPage specified. It must be a number.";

                actionBarPage = page;
            }

            // Logic for erasing phrases
            var phrasesToErase = new List<uint>();

            // Parse all memories
            for (uint memoryLine = 0; memoryLine < 10; memoryLine++)
            {
                for (uint memoryIndex = 0; memoryIndex < PhraseManager.PHRASE_MAX_MEMORY_SLOT; memoryIndex++)
                {
                    var phraseId = ryzomClient.GetPhraseManager().GetPhraseIdFromMemory(memoryLine, memoryIndex);

                    var usageCount = ryzomClient.GetPhraseManager().CountAllThatUsePhrase(phraseId);

                    // Check if the action bar page is specified, and if it matches
                    if (!actionBarPage.HasValue || actionBarPage.Value == memoryLine)
                    {
                        // Only erase the phrases that are not used anywhere else
                        if (usageCount == 1)
                            phrasesToErase.Add(phraseId);

                        // Send forget to server for this memory line and index
                        ryzomClient.GetPhraseManager().SendForgetToServer(memoryLine, memoryIndex);
                    }
                }
            }

            // Now erase the phrases that are marked for deletion
            foreach (var phraseId in phrasesToErase)
            {
                ryzomClient.GetPhraseManager().ErasePhrase(phraseId);
            }

            ryzomClient.GetNetworkManager().Update();

            return "Phrases erased successfully.";
        }

        public override IEnumerable<string> GetCmdAliases()
        {
            return ["DeletePhrases"];
        }
    }
}
