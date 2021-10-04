using RCC.Commands.Internal;
using RCC.Network;
using System.Collections.Generic;

namespace RCC.Commands
{
    public class Target : CommandBase
    {
        public override string CmdName => "target";
        public override string CmdUsage => "<name>";
        public override string CmdDesc => "Finds the nearest entity whose name contains the given string.";

        public override string Run(RyzomClient handler, string command, Dictionary<string, object> localVars)
        {
            var args = GetArgs(command);

            //var entityName = "";

            if (args.Length != 0)
            {
                //entityName = string.Join(" ", args);
            }

            //Entity.Entity entity = handler.GetNetworkManager().GetEntityManager().GetEntityByName(entityName, false, true);

            return "not implemented!";
        }

        public override IEnumerable<string> GetCmdAliases()
        {
            return new[] { "tar" };
        }
    }
}