using System;
using System.Collections.Generic;
using System.IO;
using API;
using API.Commands;
using Client.Phrase;
using Client.Sheet;

namespace Client.Commands
{
    public class PhraseImport : CommandBase
    {
        public override string CmdName => "PhraseImport";

        public override string CmdUsage => "<filename> [page]";

        public override string CmdDesc => "Import a phrase from a file. Optionally specify an action bar page.";

        public override bool Run(IClient handler, string command, out string responseMsg, Dictionary<string, object> localVars)
        {
            responseMsg = "";
            if (handler is not RyzomClient ryzomClient)
                throw new Exception("Command handler is not a Ryzom client.");

            var args = GetArgs(command);
            if (args.Length is < 1 or > 2)
            {
                responseMsg = "Please specify a file name and optionally an ActionBarPage.";
                return false;
            }

            if (!File.Exists(args[0]))
            {
                responseMsg = "File does not exist.";
                return false;
            }

            uint? actionBarPage = null;
            if (args.Length == 2)
            {
                if (!uint.TryParse(args[1], out var page))
                {
                    responseMsg = "Invalid ActionBarPage specified. It must be a number.";
                    return true;
                }

                actionBarPage = page;
            }

            var lines = File.ReadLines(args[0]);

            PhraseCom phrase = null;
            uint phraseId = 0;
            uint memoryLine = 0;
            uint memoryIndex = 0;

            // Change this line to initialize within the loop
            foreach (var line in lines)
            {
                var splits = line.Split("\t");

                if (line.Trim().StartsWith('#'))
                {
                    // Ignore comments
                }
                else if (line.StartsWith('\t'))
                {
                    // Line starts with a tab -> Brick
                    if (phrase == null)
                        continue;

                    var id = uint.Parse(splits[1]);

                    var sheet = ryzomClient.GetSheetIdFactory().SheetId(id);
                    phrase.Bricks.Add((SheetId)sheet);
                }
                else
                {
                    // First, send the previous phrase to the server if it exists
                    if (!actionBarPage.HasValue || memoryLine == actionBarPage.Value)
                        SendPhraseToServer(ryzomClient, phrase, memoryLine, memoryIndex, phraseId, out responseMsg);

                    phraseId = 0;
                    memoryLine = 0;
                    memoryIndex = 0;

                    if (splits.Length <= 5)
                        continue;

                    memoryLine = uint.Parse(splits[0].Split(":")[0]);
                    memoryIndex = uint.Parse(splits[0].Split(":")[1]);

                    // Get a new phrase ID
                    phraseId = ryzomClient.GetPhraseManager().AllocatePhraseSlot();

                    phrase = new PhraseCom();
                    ryzomClient.GetPhraseManager().SetPhraseNoUpdateDb((ushort)phraseId, phrase);

                    if (!splits[4].StartsWith('<'))
                        phrase.Name = splits[4];
                }
            }

            // Send the last phrase to the server after EOF
            if (!actionBarPage.HasValue || memoryLine == actionBarPage.Value)
                SendPhraseToServer(ryzomClient, phrase, memoryLine, memoryIndex, phraseId, out responseMsg);

            return true;
        }

        /// <summary>
        /// Updates the server with a new phrase by first removing an existing phrase from memory,
        /// then adding the new phrase to the specified memory line and index.
        /// </summary>
        private static void SendPhraseToServer(RyzomClient ryzomClient, PhraseCom phrase, uint memoryLine, uint memoryIndex, uint phraseId, out string responseMsg)
        {
            if (phrase == null || phrase.Bricks.Count <= 0)
            {
                responseMsg = "Empty phrase or bricks count.";
                return;
            }

            // Check for existing phrase ID in the specified memory line and index
            var existingPhraseId = ryzomClient.GetPhraseManager().GetPhraseIdFromMemory(memoryLine, memoryIndex);

            if (existingPhraseId == 0)
            {
                // learn and add to action bar
                responseMsg = $"§aImporting phrase {(phrase.Name.Length > 0 ? $"'{phrase.Name}'" : $"{phraseId}")} to memory line {memoryLine} slot {memoryIndex}.";

                ryzomClient.GetPhraseManager().SendLearnToServer(phraseId);
                ryzomClient.GetPhraseManager().SetPhraseInternal(phraseId, ryzomClient.GetPhraseManager().GetPhrase(phraseId), false, false);
                ryzomClient.GetNetworkManager().Update();

                ryzomClient.GetPhraseManager().SendMemorizeToServer(memoryLine, memoryIndex, phraseId);
            }
            else
            {
                responseMsg = $"Importing phrase {(phrase.Name.Length > 0 ? $"'{phrase.Name}'" : $"{phraseId}")} to memory line {memoryLine} slot {memoryIndex} failed. Already a phrase at this slot.";
            }
        }

        public override IEnumerable<string> GetCmdAliases()
        {
            return ["ImportPhrase"];
        }
    }
}