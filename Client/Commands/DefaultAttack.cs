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

        public override bool Run(IClient handler, string command, out string responseMsg, Dictionary<string, object> localVars)
        {
            responseMsg = "";
            if (handler is not RyzomClient ryzomClient)
                throw new Exception("Command handler is not a Ryzom client.");

            // Default attack on the current selection.
            var userEntity = ryzomClient.GetApiNetworkManager().GetApiEntityManager().GetApiUserEntity();
            userEntity.Attack();

            // Well Done.
            return true;
        }
    }
}