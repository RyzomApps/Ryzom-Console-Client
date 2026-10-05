using API;
using API.Commands;
using Client.Phrase;
using Client.Sheet;
using System;
using System.Collections.Generic;
using System.IO;

namespace Client.Commands
{
    public class PhraseImport : CommandBase
    {
        public override string CmdName => "PhraseImport";

        public override string CmdUsage => "<filename> [page] [override]";

        public override string CmdDesc => "Import a phrase from a file. Optionally specify an action bar page, and whether to override an existing phrase at the slot.";

        public override bool Run(IClient handler, string command, out string responseMsg, Dictionary<string, object> localVars)
        {
            responseMsg = "";
            if (handler is not RyzomClient ryzomClient)
                throw new Exception("Command handler is not a Ryzom client.");

            var args = GetArgs(command);
            if (args.Length is < 1 or > 3)
            {
                responseMsg = "Please specify a file name and optionally an ActionBarPage and override (true/false).";
                return false;
            }

            if (!File.Exists(args[0]))
            {
                responseMsg = "File does not exist.";
                return false;
            }

            uint? actionBarPage = null;
            if (args.Length >= 2)
            {
                if (!uint.TryParse(args[1], out var page))
                {
                    responseMsg = "Invalid ActionBarPage specified. It must be a number.";
                    return true;
                }

                actionBarPage = page;
            }

            bool overrideSlot = args.Length >= 3 &&
                                (args[2].Equals("true", StringComparison.OrdinalIgnoreCase) ||
                                 args[2].Equals("1"));

            // BOM handling: File.ReadLines + strip the BOM from the first line manually.
            bool firstLine = true;

            PhraseCom phrase = null;
            uint phraseId = 0;
            uint memoryLine = 0;
            uint memoryIndex = 0;
            bool phraseMatchesPage = true; // no page filter -> always import

            // Change this line to initialize within the loop
            foreach (var rawLine in File.ReadLines(args[0]))
            {
                var line = firstLine ? rawLine.TrimStart('\uFEFF') : rawLine;
                firstLine = false;

                var splits = line.Split('\t');

                if (line.Trim().StartsWith('#'))
                {
                    // Ignore comments
                }
                else if (line.StartsWith('\t'))
                {
                    // Line starts with a tab -> Brick
                    if (phrase == null)
                        continue;

                    if (!uint.TryParse(splits[1], out var id))
                    {
                        responseMsg = $"Invalid brick id '{splits[1]}' in phrase '{(phrase.Name.Length > 0 ? phrase.Name : phraseId.ToString())}'.";
                        return false;
                    }

                    var sheet = ryzomClient.GetSheetIdFactory().SheetId(id);

                    // Guard against unknown sheets: the import would otherwise report
                    // success and the server would silently reject the phrase.
                    if (sheet.ToString().StartsWith("unknown", StringComparison.OrdinalIgnoreCase))
                    {
                        responseMsg = $"Unknown brick sheet id {id} in phrase '{(phrase.Name.Length > 0 ? phrase.Name : phraseId.ToString())}'.";
                        return false;
                    }

                    phrase.Bricks.Add((SheetId)sheet);
                }
                else
                {
                    // Skip blank lines: they would otherwise flush the previous phrase
                    // twice and report a bogus "Already a phrase at this slot." error.
                    if (line.Trim().Length == 0)
                        continue;

                    // First, send the previous phrase to the server if it exists
                    if (phrase != null && phraseMatchesPage)
                        SendPhraseToServer(ryzomClient, phrase, memoryLine, memoryIndex, phraseId, overrideSlot, out responseMsg);

                    phraseId = 0;
                    memoryLine = 0;
                    memoryIndex = 0;
                    phrase = null;
                    phraseMatchesPage = true;

                    if (splits.Length <= 5)
                        continue;

                    memoryLine = uint.Parse(splits[0].Split(":")[0]);
                    memoryIndex = uint.Parse(splits[0].Split(":")[1]);

                    // Page filter: skip allocation for phrases not on the requested page
                    if (actionBarPage.HasValue && memoryLine != actionBarPage.Value)
                    {
                        phraseMatchesPage = false;
                        continue;
                    }

                    // Get a new phrase ID
                    phraseId = ryzomClient.GetPhraseManager().AllocatePhraseSlot();

                    phrase = new PhraseCom();
                    ryzomClient.GetPhraseManager().SetPhraseNoUpdateDb((ushort)phraseId, phrase);

                    if (!splits[4].StartsWith('<'))
                        phrase.Name = splits[4];
                }
            }

            // Send the last phrase to the server after EOF
            if (phrase != null && phraseMatchesPage)
                SendPhraseToServer(ryzomClient, phrase, memoryLine, memoryIndex, phraseId, overrideSlot, out responseMsg);

            return true;
        }

        /// <summary>
        /// Updates the server with a new phrase by first removing an existing phrase from memory,
        /// then adding the new phrase to the specified memory line and index.
        /// </summary>
        private static void SendPhraseToServer(RyzomClient ryzomClient, PhraseCom phrase, uint memoryLine, uint memoryIndex, uint phraseId, bool overrideSlot, out string responseMsg)
        {
            if (phrase == null || phrase.Bricks.Count <= 0)
            {
                responseMsg = $"Empty phrase or bricks count in bar {memoryLine} at slot {memoryIndex}.";
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
            else if (overrideSlot)
            {
                // Learn the new phrase, then overwrite the existing memory slot.
                responseMsg = $"§aImporting phrase {(phrase.Name.Length > 0 ? $"'{phrase.Name}'" : $"{phraseId}")} to memory line {memoryLine} slot {memoryIndex} (override).";

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
