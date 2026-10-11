using API;
using API.Commands;
using System;
using System.Collections.Generic;

namespace Client.Commands
{
    /// <summary>
    /// Show phrases of a memory line, optionally the bricks of one slot.
    /// </summary>
    public class PhraseShow : CommandBase
    {
        public override CommandCategory CmdCategory => CommandCategory.Combat;

        public override string CmdName => "ShowPhrase";

        public override string CmdUsage => "<line> [index]";

        public override string CmdDesc => "Show the phrases of a memory line. When an index is given, also show the bricks of that phrase.";

        public override bool Run(IClient handler, string command, out string responseMsg, Dictionary<string, object> localVars)
        {
            responseMsg = "";
            if (handler is not RyzomClient ryzomClient)
                throw new Exception("Command handler is not a Ryzom client.");

            var args = GetArgs(command);
            if (args.Length is < 1 or > 2)
            {
                responseMsg = $"Usage: {CmdUsage}";
                return false;
            }

            if (!uint.TryParse(args[0], out var line))
            {
                responseMsg = "Invalid memory line. It must be a number.";
                return false;
            }

            uint? index = null;
            if (args.Length >= 2)
            {
                if (!uint.TryParse(args[1], out var i))
                {
                    responseMsg = "Invalid index. It must be a number.";
                    return false;
                }

                index = i;
            }

            var phrases = ryzomClient.GetPhraseManager();
            var memories = phrases.GetMemories();

            if (line >= memories.Count)
            {
                responseMsg = $"Memory line {line} does not exist (0..{memories.Count - 1}).";
                return false;
            }

            var slots = memories[(int)line].Slot;

            if (index.HasValue)
            {
                if (index.Value >= slots.Length)
                {
                    responseMsg = $"Slot {index} does not exist (0..{slots.Length - 1}).";
                    return false;
                }

                var slot = slots[index.Value];
                if (slot.Id == 0)
                {
                    responseMsg = $"Line {line}, slot {index} is empty.";
                    return true;
                }

                var phrase = phrases.GetPhrase(slot.Id);
                var lines = new List<string>
                {
                    $"{line}:{index}\t{phrase.Name}\t({phrase.Bricks?.Count ?? 0} bricks)"
                };

                if (phrase.Bricks != null)
                {
                    foreach (var brick in phrase.Bricks)
                    {
                        lines.Add($"\t{brick.AsInt()}\t{brick.GetShortId()}\t{brick.GetSheetType()}\t{brick.Name}");
                    }
                }

                responseMsg = string.Join("\n", lines);
                return true;
            }

            var outLines = new List<string> { $"Memory line {line}:" };

            for (var s = 0; s < slots.Length; s++)
            {
                var slot = slots[s];
                if (slot.Id == 0)
                    continue;

                var phrase = phrases.GetPhrase(slot.Id);
                outLines.Add($"{line}:{s}\t{phrase.Name}");
            }

            responseMsg = string.Join("\n", outLines);
            return true;
        }

        public override IEnumerable<string> GetCmdAliases()
        {
            return ["PhraseShow"];
        }
    }
}
