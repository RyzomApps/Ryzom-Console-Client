using System.Collections.Generic;
using System.Numerics;
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

            var userPos = entityManager.GetApiUserEntity().Pos;

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

            // Iterate players
            foreach (var entity in entityManager.GetApiEntities())
            {
                if (entity == null || entity.GetEntityType() != EntityType.Player)
                    continue;

                var playerPos = entity.Pos;

                ret += $"{entity.Slot()}\t{(entity.GetDisplayName().Trim().Length > 0 ? entity.GetDisplayName() : "[Unnamed]")} ({entity.GetGuildName()})\t{Vector3.Distance(playerPos, userPos):0} m\n";
                count++;
            }

            ret = $"There is/are {count} player(s) around:\n{ret}";

            // Iterate team
            var databaseManager = handler.GetApiDatabaseManager();
            var stringManager = handler.GetApiStringManager();
            var networkManager = handler.GetApiNetworkManager();

            if (databaseManager == null)
                return ret[..^1];

            var retTeam = "";
            count = 0;

            for (var gm = 0; gm < 7; gm++)
            {
                var present = databaseManager.GetProp($"SERVER:GROUP:{gm}:PRESENT");

                if (present == 0)
                    break;

                var nameId = databaseManager.GetProp($"SERVER:GROUP:{gm}:NAME");

                if (stringManager != null && networkManager != null)
                {
                    stringManager.GetString((uint)nameId, out var name, networkManager);
                    name = EntityHelper.RemoveTitleAndShardFromName(name);

                    if (name.Trim().Length == 0)
                        continue;

                    var posId = databaseManager.GetProp($"SERVER:GROUP:{gm}:POS");

                    var y = (int)(posId & uint.MaxValue) / 1000f;
                    var x = (int)(posId >> 32) / 1000f;

                    var teamPos = new Vector3(x, y, userPos.Z);

                    retTeam += $"{gm}\t{name}\t{Vector3.Distance(teamPos, userPos):0} m\n";
                }
                else
                {
                    retTeam += $"{gm}\t{nameId}\n";
                }

                count++;
            }

            retTeam = $"There is/are {count} team member(s):\n{retTeam}";

            return ret + retTeam[..^1];
        }

        public override IEnumerable<string> GetCmdAliases()
        {
            return new[] { "" };
        }
    }
}