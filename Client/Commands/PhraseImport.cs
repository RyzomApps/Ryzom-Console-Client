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
                string[] splits = line.Split("\t");

                if (line.StartsWith("\t"))
                {
                    if (phrase != null)
                    {
                        uint id = uint.Parse(splits[1]);

                        ISheetId sheet = ryzomClient.GetSheetIdFactory().SheetId(id);
                        //var bs = ryzomClient.GetSheetManager().Get(sheet);
                        phrase.Bricks.Add((SheetId)sheet);
                    }
                }
                else
                {
                    if (phrase != null)
                    {
                        ryzomClient.GetLogger().Info($"Importing phrase {phraseId} to memory line {memoryLine} slot {memoryIndex}.");

                        // forget
                        ryzomClient.GetPhraseManager().SendForgetToServer(memoryLine, memoryIndex);

                        // learn
                        ryzomClient.GetPhraseManager().SendLearnToServer(phraseId);

                        // commit
                        ryzomClient.GetPhraseManager().SendMemorizeToServer(memoryLine, memoryIndex, phraseId);
                    }

                    if (splits.Length > 5)
                    {
                        phraseId = ryzomClient.GetPhraseManager().AllocatePhraseSlot();

                        phrase = new PhraseCom();
                        ryzomClient.GetPhraseManager().SetPhraseNoUpdateDb((ushort)phraseId, phrase);

                        var MemSlot = splits[0].Split(":");
                        memoryLine = uint.Parse(MemSlot[0]);
                        memoryIndex = uint.Parse(MemSlot[1]);
                        phrase.Name = splits[4];
                    }
                }
            }

            return "";
        }

        public override IEnumerable<string> GetCmdAliases()
        {
            return new string[] { };
        }
    }
}