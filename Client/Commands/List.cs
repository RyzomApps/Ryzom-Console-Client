using System.Collections.Generic;
using API;
using API.Commands;
using API.Entity;

namespace Client.Commands
{
    public class List : CommandBase
    {
        public override string CmdName => "list";
        public override string CmdUsage => "";
        public override string CmdDesc => "Allows the user to list all players that are around";

        public override string Run(IClient handler, string command, Dictionary<string, object> localVars)
        {
            var ret = "";
            var count = 0;

            var entityManager = handler.GetApiNetworkManager()?.GetApiEntityManager();

            // Iterate players
            if (entityManager == null)
                return "Entity manager not initialized.";

            foreach (var entity in entityManager.GetApiEntities())
            {
                // Entity must be defined - Not the player
                if (entity == null || entity.Slot() == 0)
                    continue;

                // Try to get the name with a tar command (if the player is to far away) - for the next update
                if (entity.GetDisplayName().Trim() != "" || entity.GetEntityType() != EntityType.Player)
                    continue;

                var temp = "";
                handler.PerformInternalCommand($"tar {entity.Slot()}", ref temp, localVars);
            }

            foreach (var entity in entityManager.GetApiEntities())
            {
                if (entity == null || entity.GetEntityType() != EntityType.Player)
                    continue;

                ret += $"{entity.Slot()}\t{(entity.GetDisplayName().Trim().Length > 0 ? entity.GetDisplayName() : "[Unnamed]")} ({entity.GetGuildName()})\n";
                count++;
            }

            ret = $"There are {count} player(s) around:\n{ret}";

            return ret[..^1];
        }

        public override IEnumerable<string> GetCmdAliases()
        {
            return new[] { "" };
        }
    }
}