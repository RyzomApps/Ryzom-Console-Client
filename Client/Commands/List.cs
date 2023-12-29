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

            var _entityManager = handler.GetApiNetworkManager()?.GetApiEntityManager();

            // Iterate players
            foreach (var entity in _entityManager.GetApiEntities())
            {
                if (entity == null)
                    continue;

                if (entity.GetEntityType() != EntityType.Player)
                    continue;

                ret += $"{(entity.GetDisplayName().Trim().Length > 0 ? entity.GetDisplayName() : "[Unnamed]")} ({entity.GetGuildName()})\n";
                count++;
            }

            ret = $"There are {count} player(s) around:\n{ret}";

            return ret[0..^1];
        }

        public override IEnumerable<string> GetCmdAliases()
        {
            return new[] { "" };
        }
    }
}