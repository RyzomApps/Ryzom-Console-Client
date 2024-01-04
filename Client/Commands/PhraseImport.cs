using System;
using System.Collections.Generic;
using System.IO;
using API;
using API.Commands;
using API.Sheet;
using Client.Brick;
using Client.Phrase;
using Client.Sheet;

namespace Client.Commands
{
    public class PhraseImport : CommandBase
    {
        public override string CmdName => "PhraseImport";

        public override string CmdUsage => "<filename>";

        public override string CmdDesc => "Import a phrase from a file.";

        public override string Run(IClient handler, string command, Dictionary<string, object> localVars)
        {
            if (!(handler is RyzomClient ryzomClient))
                throw new Exception("Command handler is not a Ryzom client.");

            var args = GetArgs(command);

            if (args.Length != 1)
                return "Please specify a file name.";

            if (!File.Exists(args[0]))
                return "File does not exist.";

            var lines = File.ReadLines(args[0]);

            PhraseCom phrase = null;
            uint phraseId = 0;
            uint memoryLine = 0;
            uint memoryIndex = 0;

            foreach (var line in lines)
            {
                var splits = line.Split("\t");

                if (line.StartsWith("\t"))
                {
                    if (phrase == null)
                        continue;

                    var id = uint.Parse(splits[1]);

                    var sheet = ryzomClient.GetSheetIdFactory().SheetId(id);
                    phrase.Bricks.Add((SheetId)sheet);
                }
                else
                {
                    SendPhraseToServer(ryzomClient, ref phrase, ref memoryLine, ref memoryIndex, ref phraseId);

                    if (splits.Length <= 5)
                        continue;

                    phraseId = ryzomClient.GetPhraseManager().AllocatePhraseSlot();

                    phrase = new PhraseCom();
                    ryzomClient.GetPhraseManager().SetPhraseNoUpdateDb((ushort)phraseId, phrase);

                    var memory = splits[0].Split(":");
                    memoryLine = uint.Parse(memory[0]);
                    memoryIndex = uint.Parse(memory[1]);

                    if (!splits[4].StartsWith("<"))
                        phrase.Name = splits[4];
                }
            }

            SendPhraseToServer(ryzomClient, ref phrase, ref memoryLine, ref memoryIndex, ref phraseId);

            return "";
        }

        /// <summary>
        /// forget memory<br/>
        /// learn phrase<br/>
        /// memorize phrase
        /// </summary>
        private static void SendPhraseToServer(RyzomClient ryzomClient, ref PhraseCom phrase, ref uint memoryLine, ref uint memoryIndex, ref uint phraseId)
        {
            if (phrase == null)
                return;

            // server forget
            ryzomClient.GetPhraseManager().SendForgetToServer(memoryLine, memoryIndex);

            if (phrase.Bricks.Count > 0)
            {
                // add the new phrase
                ryzomClient.GetLogger().Info($"Importing phrase {(phrase.Name.Length > 0 ? phrase.Name : $"{phraseId}")} to memory line {memoryLine} slot {memoryIndex}.");

                // server - learn
                ryzomClient.GetPhraseManager().SendLearnToServer(phraseId);

                // server - add to action bar
                ryzomClient.GetPhraseManager().SendMemorizeToServer(memoryLine, memoryIndex, phraseId);
            }

            phrase = null;
            phraseId = 0;
            memoryLine = 0;
            memoryIndex = 0;
        }

        public override IEnumerable<string> GetCmdAliases()
        {
            return new string[] { "ImportPhrase" };
        }
    }
}