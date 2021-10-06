using System.Collections.Generic;
using RCC.Commands.Internal;

namespace RCC.Commands
{
    public class LogEntities : CommandBase
    {
        public override string CmdName => "logEntities";

        public override string CmdUsage => "";

        public override string CmdDesc =>
            "Write the position and orientation af all entities in the vision in the file 'entities.txt'";

        public override string Run(RyzomClient handler, string command, Dictionary<string, object> localVars)
        {
            var args = GetArgs(command);

            // Check parameters
            if (args.Length != 0) return "";

            // Log entities
            handler.GetNetworkManager().GetEntityManager().WriteEntities();

            // Command well done.
            return "";
        }

        public override IEnumerable<string> GetCmdAliases()
        {
            return new string[] { };
        }
    }
}