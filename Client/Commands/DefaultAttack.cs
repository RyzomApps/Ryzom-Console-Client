using System;
using System.Collections.Generic;
using API;
using API.Commands;

namespace Client.Commands
{
    /// <summary>
    /// Use default attack on target
    /// </summary>
    public class DefaultAttack : CommandBase
    {
        public override string CmdName => "defaultAttack";

        public override string CmdUsage => "";

        public override string CmdDesc => "Use default attack on target";

        public override string Run(IClient handler, string command, Dictionary<string, object> localVars)
        {
            if (!(handler is RyzomClient ryzomClient))
                throw new Exception("Command handler is not a Ryzom client.");

            // Default attack on the current selection.
            var userEntity = ryzomClient.GetApiNetworkManager().GetApiEntityManager().GetApiUserEntity();
            userEntity.Attack();

            // Well Done.
            return "";
        }

        public override IEnumerable<string> GetCmdAliases()
        {
            return new string[] { };
        }
    }
}