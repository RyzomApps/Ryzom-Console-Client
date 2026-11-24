///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////
using API;
using API.Commands;
using System;
using System.Collections.Generic;

namespace Client.Commands
{
    /// <summary>
    /// Command to write a file with the position and orientation of all entities
    /// in the vision, for the map tools and for analysis.
    /// </summary>
    public class LogEntities : CommandBase
    {

        public override CommandCategory CmdCategory => CommandCategory.Debug;
        public override string CmdName => "logEntities";

        public override string CmdUsage => "[output_file]";

        public override string CmdDesc =>
            "Write the position and orientation of all entities in the vision to a file.\n" +
            "Without argument the legacy format is written to 'entities.txt'.\n" +
            "A file name ending in .json writes a full JSON dump (slot, dataset id, npc alias,\n" +
            "sheet, name, title, guild, type, position, heading, dead flag, properties), after\n" +
            "asking the string manager for the name texts of the server NPCs in the vision.";

        public override bool Run(IClient handler, string command, out string responseMsg, Dictionary<string, object> localVars)
        {
            responseMsg = "";
            if (handler is not RyzomClient ryzomClient)
                throw new Exception("Command handler is not a Ryzom client.");

            var args = GetArgs(command);

            // Check parameters: 0 args = legacy file, 1 arg = explicit output file
            if (args.Length > 1)
            {
                responseMsg = $"Usage: {CmdUsage}";
                return false;
            }

            var file = args.Length == 1 ? args[0] : "entities.txt";
            var json = file.EndsWith(".json", StringComparison.OrdinalIgnoreCase);

            var entityManager = ryzomClient.GetNetworkManager().GetEntityManager();

            // The name and the guild of a server NPC are not part of its entity properties:
            // they are plain string ids and the server only sends their texts on request
            // through the string manager. Ask for them and wait a short time, the legacy
            // format has no names, so nothing to warm up there.
            Dictionary<uint, (string Name, string Guild)> npcNames = null;

            if (json)
                npcNames = entityManager.WarmUpNpcNames();

            // Log entities
            entityManager.WriteEntities(file, npcNames);

            responseMsg = $"Entities written to '{file}'.";

            // Unresolved texts are written as empty strings: say it, the warm-up waits
            // only a limited time for the answers of the string manager.
            if (npcNames != null)
            {
                var missing = 0;

                foreach (var entry in npcNames)
                    if (entry.Value.Name.Length == 0)
                        ++missing;

                if (missing > 0)
                    responseMsg += $"\nNamen evtl. unvollständig ({missing} npc-Namen fehlen): 'dumpEntitiesName' vorab ausführen.";
            }

            return true;
        }
    }

    /// <summary>
    /// Command to resolve the name and guild texts of the server NPCs in the vision without
    /// writing a file, a companion of 'logEntities &lt;output_file&gt;.json'. Run it before the
    /// dump (or after a teleport) so the dump finds every text in the cache of the string
    /// manager and does not have to wait for the answers of the server.
    /// </summary>
    public class DumpEntitiesName : CommandBase
    {
        public override CommandCategory CmdCategory => CommandCategory.Debug;

        public override string CmdName => "dumpEntitiesName";

        public override string CmdUsage => "[npc_alias ...]";

        public override string CmdDesc =>
            "Ask the string manager for the name and guild texts of the server NPCs in the vision\n" +
            "and report them, without writing a file. The names of server NPCs are not part of\n" +
            "their entity properties, the server sends them only on request\n" +
            "(STRING_MANAGER:STRING_RQ), so run this before 'logEntities <output_file>.json' to\n" +
            "get complete names in the dump. With npc aliases as arguments only those are resolved.";

        public override bool Run(IClient handler, string command, out string responseMsg, Dictionary<string, object> localVars)
        {
            responseMsg = "";
            if (handler is not RyzomClient ryzomClient)
                throw new Exception("Command handler is not a Ryzom client.");

            var args = GetArgs(command);

            // No arguments: every NPC entity in the vision
            ICollection<uint> onlyAliases = null;

            if (args.Length > 0)
            {
                var aliases = new List<uint>(args.Length);

                foreach (var arg in args)
                {
                    if (!uint.TryParse(arg, out var alias) || alias == 0)
                    {
                        responseMsg = $"Invalid npc alias '{arg}'";
                        return false;
                    }

                    if (!aliases.Contains(alias))
                        aliases.Add(alias);
                }

                onlyAliases = aliases;
            }

            var npcNames = ryzomClient.GetNetworkManager().GetEntityManager()
                .WarmUpNpcNames(onlyAliases: onlyAliases);

            var aliasesSorted = new List<uint>(npcNames.Keys);
            aliasesSorted.Sort();

            var lines = new List<string>();
            var missing = 0;

            foreach (var alias in aliasesSorted)
            {
                var info = npcNames[alias];

                if (info.Name.Length == 0)
                    ++missing;

                lines.Add($"  alias {alias}: name='{info.Name}' guild='{info.Guild}'");
            }

            // Aliases that were asked for but have no entity in the vision: the string ids
            // of an npc are only known from its entity, so nothing can be requested for them.
            if (onlyAliases != null)
            {
                foreach (var alias in onlyAliases)
                {
                    if (!npcNames.ContainsKey(alias))
                        lines.Add($"  alias {alias}: no npc entity with this alias in the vision");
                }
            }

            if (npcNames.Count == 0 && lines.Count == 0)
            {
                responseMsg = "No npc entity in the vision.";
                return true;
            }

            var header = $"{npcNames.Count} npc alias(es), {missing} unresolved name(s)";

            if (missing > 0)
                header += ": unresolved names are written empty in the dump";

            responseMsg = header + "\n" + string.Join("\n", lines);

            return true;
        }
    }
}
